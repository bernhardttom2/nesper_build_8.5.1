///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumGroupByKeyValueSelectorEventsForgeEval : EnumEval
    {
        private readonly EnumGroupByKeyValueSelectorEventsForge forge;
        private readonly ExprEvaluator innerExpression;
        private readonly ExprEvaluator secondExpression;

        public EnumGroupByKeyValueSelectorEventsForgeEval(
            EnumGroupByKeyValueSelectorEventsForge forge,
            ExprEvaluator innerExpression,
            ExprEvaluator secondExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
            this.secondExpression = secondExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return Collections.GetEmptyMap<object, object>();
            }

            IDictionary<object, ICollection<object>> result = new LinkedHashMap<object, ICollection<object>>();

            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            foreach (EventBean next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                object key = innerExpression.Evaluate(eventsLambda, isNewData, context);
                object entry = secondExpression.Evaluate(eventsLambda, isNewData, context);

                ICollection<object> value = result.Get(key);
                if (value == null) {
                    value = new List<object>();
                    result.Put(key, value);
                }

                value.Add(entry);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumGroupByKeyValueSelectorEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(IDictionary<object, object>),
                    typeof(EnumGroupByKeyValueSelectorEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(object), typeof(object) }))
                .DeclareVar<IDictionary<object, object>>("result", NewInstance(typeof(LinkedHashMap<object, object>)));
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar<object>(
                    "key",
                    forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar<object>(
                    "entry",
                    forge.secondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar<ICollection<object>>(
                    "value",
                    Cast(typeof(ICollection<object>), ExprDotMethod(@Ref("result"), "Get", @Ref("key"))))
                .IfRefNull("value")
                .AssignRef("value", NewInstance(typeof(List<object>)))
                .Expression(ExprDotMethod(@Ref("result"), "Put", @Ref("key"), @Ref("value")))
                .BlockEnd()
                .Expression(ExprDotMethod(@Ref("value"), "Add", @Ref("entry")));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace