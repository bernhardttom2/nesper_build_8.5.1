///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.module.EPStatementInitServices;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class BindSelectExprProcessorForge : SelectExprProcessorForge
    {
        private readonly SelectExprProcessorForge syntheticProcessorForge;
        private readonly BindProcessorForge bindProcessorForge;

        public BindSelectExprProcessorForge(
            SelectExprProcessorForge syntheticProcessorForge,
            BindProcessorForge bindProcessorForge)
        {
            this.syntheticProcessorForge = syntheticProcessorForge;
            this.bindProcessorForge = bindProcessorForge;
        }

        public EventType ResultEventType {
            get => syntheticProcessorForge.ResultEventType;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod processMethod = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope);

            CodegenExpressionRef isSythesize = selectSymbol.GetAddSynthesize(processMethod);
            CodegenMethod syntheticMethod = syntheticProcessorForge.ProcessCodegen(
                resultEventType, eventBeanFactory, processMethod, selectSymbol, exprSymbol, codegenClassScope);
            CodegenMethod bindMethod = bindProcessorForge.ProcessCodegen(processMethod, exprSymbol, codegenClassScope);
            CodegenExpression isNewData = exprSymbol.GetAddIsNewData(processMethod);
            CodegenExpression exprCtx = exprSymbol.GetAddExprEvalCtx(processMethod);

            CodegenExpressionField stmtResultSvc = codegenClassScope.AddFieldUnshared(
                true, typeof(StatementResultService), ExprDotMethod(
                    EPStatementInitServicesConstants.REF,
                    EPStatementInitServicesConstants.GETSTATEMENTRESULTSERVICE));
            processMethod.Block
                .DeclareVar(typeof(bool), "makeNatural", ExprDotMethod(stmtResultSvc, "isMakeNatural"))
                .DeclareVar(typeof(bool), "synthesize", Or(ExprDotMethod(stmtResultSvc, "isMakeSynthetic"), isSythesize))
                .IfCondition(Not(@Ref("makeNatural")))
                .IfCondition(@Ref("synthesize"))
                .DeclareVar(typeof(EventBean), "synthetic", LocalMethod(syntheticMethod))
                .BlockReturn(@Ref("synthetic"))
                .BlockReturn(ConstantNull())
                .DeclareVar(typeof(EventBean), "syntheticEvent", ConstantNull())
                .IfCondition(@Ref("synthesize"))
                .AssignRef("syntheticEvent", LocalMethod(syntheticMethod))
                .BlockEnd()
                .DeclareVar(typeof(object[]), "parameters", LocalMethod(bindMethod))
                .MethodReturn(NewInstance(typeof(NaturalEventBean), resultEventType, @Ref("parameters"), @Ref("syntheticEvent")));

            return processMethod;
        }
    }
} // end of namespace