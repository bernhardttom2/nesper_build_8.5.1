///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRelationalOpAllAnyNodeForgeEval : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;

        private readonly ExprRelationalOpAllAnyNodeForge forge;

        public ExprRelationalOpAllAnyNodeForgeEval(
            ExprRelationalOpAllAnyNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
            return result;
        }

        private bool? EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (evaluators.Length == 1) {
                return false;
            }

            var isAll = forge.ForgeRenderable.IsAll;
            RelationalOpEnumComputer computer = forge.Computer;
            var valueLeft = evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            var len = evaluators.Length - 1;

            if (forge.IsCollectionOrArray) {
                var hasNonNullRow = false;
                var hasRows = false;
                for (var i = 1; i <= len; i++) {
                    var valueRight = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                    if (valueRight == null) {
                        continue;
                    }

                    if (valueRight is Array valueRightArray) {
                        hasRows = true;
                        var arrayLength = valueRightArray.Length;
                        for (var index = 0; index < arrayLength; index++) {
                            object item = valueRightArray.GetValue(index);
                            if (item == null) {
                                if (isAll) {
                                    return null;
                                }

                                continue;
                            }

                            hasNonNullRow = true;
                            if (valueLeft != null) {
                                if (isAll) {
                                    if (!computer.Compare(valueLeft, item)) {
                                        return false;
                                    }
                                }
                                else {
                                    if (computer.Compare(valueLeft, item)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (valueRight is IDictionary<object, object>) {
                        var coll = (IDictionary<object, object>) valueRight;
                        hasRows = true;
                        foreach (object item in coll.Keys) {
                            if (!(item.IsNumber())) {
                                if (isAll && item == null) {
                                    return null;
                                }

                                continue;
                            }

                            hasNonNullRow = true;
                            if (valueLeft != null) {
                                if (isAll) {
                                    if (!computer.Compare(valueLeft, item)) {
                                        return false;
                                    }
                                }
                                else {
                                    if (computer.Compare(valueLeft, item)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (valueRight is ICollection<object>) {
                        var coll = (ICollection<object>) valueRight;
                        hasRows = true;
                        foreach (object item in coll) {
                            if (!(item.IsNumber())) {
                                if (isAll && item == null) {
                                    return null;
                                }

                                continue;
                            }

                            hasNonNullRow = true;
                            if (valueLeft != null) {
                                if (isAll) {
                                    if (!computer.Compare(valueLeft, item)) {
                                        return false;
                                    }
                                }
                                else {
                                    if (computer.Compare(valueLeft, item)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!(valueRight.IsNumber())) {
                        if (isAll) {
                            return null;
                        }
                    }
                    else {
                        hasNonNullRow = true;
                        if (isAll) {
                            if (!computer.Compare(valueLeft, valueRight)) {
                                return false;
                            }
                        }
                        else {
                            if (computer.Compare(valueLeft, valueRight)) {
                                return true;
                            }
                        }
                    }
                }

                if (isAll) {
                    if (!hasRows) {
                        return true;
                    }

                    if (!hasNonNullRow || valueLeft == null) {
                        return null;
                    }

                    return true;
                }

                if (!hasRows) {
                    return false;
                }

                if (!hasNonNullRow || valueLeft == null) {
                    return null;
                }

                return false;
            }
            else {
                var hasNonNullRow = false;
                var hasRows = false;
                for (var i = 1; i <= len; i++) {
                    var valueRight = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    hasRows = true;

                    if (valueRight != null) {
                        hasNonNullRow = true;
                    }
                    else {
                        if (isAll) {
                            return null;
                        }
                    }

                    if (valueRight != null && valueLeft != null) {
                        if (isAll) {
                            if (!computer.Compare(valueLeft, valueRight)) {
                                return false;
                            }
                        }
                        else {
                            if (computer.Compare(valueLeft, valueRight)) {
                                return true;
                            }
                        }
                    }
                }

                if (isAll) {
                    if (!hasRows) {
                        return true;
                    }

                    if (!hasNonNullRow || valueLeft == null) {
                        return null;
                    }

                    return true;
                }

                if (!hasRows) {
                    return false;
                }

                if (!hasNonNullRow || valueLeft == null) {
                    return null;
                }

                return false;
            }
        }

        public static CodegenExpression Codegen(
            ExprRelationalOpAllAnyNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forges = ExprNodeUtilityQuery.GetForges(forge.ForgeRenderable.ChildNodes);
            var valueLeftType = forges[0].EvaluationType;
            var isAll = forge.ForgeRenderable.IsAll;
            if (forges.Length == 1) {
                return Constant(isAll);
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprRelationalOpAllAnyNodeForgeEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar<bool>("hasNonNullRow", ConstantFalse())
                .DeclareVar(
                    valueLeftType,
                    "valueLeft",
                    forges[0].EvaluateCodegen(valueLeftType, methodNode, exprSymbol, codegenClassScope));

            for (var i = 1; i < forges.Length; i++) {
                var refforge = forges[i];
                var refname = "r" + i;
                var reftype = refforge.EvaluationType;
                block.DeclareVar(
                    reftype,
                    refname,
                    refforge.EvaluateCodegen(reftype, methodNode, exprSymbol, codegenClassScope));

                if (TypeHelper.IsImplementsInterface(reftype, typeof(ICollection<object>))) {
                    var blockIfNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    {
                        var forEach = blockIfNotNull.ForEach(typeof(object), "item", Ref(refname));
                        {
                            var ifNotNumber = forEach.IfCondition(Not(InstanceOf(Ref("item"), typeof(object))));
                            {
                                if (isAll) {
                                    ifNotNumber.IfRefNullReturnNull("item");
                                }
                            }
                            var ifNotNumberElse = ifNotNumber.IfElse();
                            {
                                ifNotNumberElse.AssignRef("hasNonNullRow", ConstantTrue());
                                var ifLeftNotNull = ifNotNumberElse.IfCondition(NotEqualsNull(Ref("valueLeft")));
                                {
                                    ifLeftNotNull.IfCondition(
                                            NotOptional(
                                                isAll,
                                                forge.Computer.Codegen(
                                                    Ref("valueLeft"),
                                                    valueLeftType,
                                                    Cast(typeof(object), Ref("item")),
                                                    typeof(object))))
                                        .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                                }
                            }
                        }
                    }
                }
                else if (TypeHelper.IsImplementsInterface(reftype, typeof(IDictionary<object, object>))) {
                    var blockIfNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    {
                        var forEach = blockIfNotNull.ForEach(
                            typeof(object),
                            "item",
                            ExprDotName(Ref(refname), "Keys"));
                        {
                            var ifNotNumber = forEach.IfCondition(Not(InstanceOf(Ref("item"), typeof(object))));
                            {
                                if (isAll) {
                                    ifNotNumber.IfRefNullReturnNull("item");
                                }
                            }
                            var ifNotNumberElse = ifNotNumber.IfElse();
                            {
                                ifNotNumberElse.AssignRef("hasNonNullRow", ConstantTrue());
                                var ifLeftNotNull = ifNotNumberElse.IfCondition(NotEqualsNull(Ref("valueLeft")));
                                {
                                    ifLeftNotNull.IfCondition(
                                            NotOptional(
                                                isAll,
                                                forge.Computer.Codegen(
                                                    Ref("valueLeft"),
                                                    valueLeftType,
                                                    Cast(typeof(object), Ref("item")),
                                                    typeof(object))))
                                        .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                                }
                            }
                        }
                    }
                }
                else if (reftype.IsArray) {
                    var blockIfNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    {
                        var forLoopArray = blockIfNotNull.ForLoopIntSimple("index", ArrayLength(Ref(refname)));
                        {
                            forLoopArray.DeclareVar(
                                Boxing.GetBoxedType(reftype.GetElementType()),
                                "item",
                                ArrayAtIndex(Ref(refname), Ref("index")));
                            var ifItemNull = forLoopArray.IfCondition(EqualsNull(Ref("item")));
                            {
                                if (isAll) {
                                    ifItemNull.IfReturn(ConstantNull());
                                }
                            }
                            var ifItemNotNull = ifItemNull.IfElse();
                            {
                                ifItemNotNull.AssignRef("hasNonNullRow", ConstantTrue());
                                var ifLeftNotNull = ifItemNotNull.IfCondition(NotEqualsNull(Ref("valueLeft")));
                                {
                                    ifLeftNotNull.IfCondition(
                                            NotOptional(
                                                isAll,
                                                forge.Computer.Codegen(
                                                    Ref("valueLeft"),
                                                    valueLeftType,
                                                    Ref("item"),
                                                    typeof(object))))
                                        .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                                }
                            }
                        }
                    }
                }
                else if (!TypeHelper.IsSubclassOrImplementsInterface(
                    Boxing.GetBoxedType(reftype),
                    typeof(object))) {
                    if (!reftype.IsPrimitive) {
                        block.IfRefNullReturnNull(refname);
                    }

                    block.AssignRef("hasNonNullRow", ConstantTrue());
                    if (isAll) {
                        block.BlockReturn(ConstantNull());
                    }
                }
                else {
                    if (reftype.IsPrimitive) {
                        block.AssignRef("hasNonNullRow", ConstantTrue());
                        block.IfCondition(
                                NotOptional(
                                    isAll,
                                    forge.Computer.Codegen(Ref("valueLeft"), valueLeftType, Ref(refname), reftype)))
                            .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                    }
                    else {
                        if (isAll) {
                            block.IfRefNullReturnNull(refname);
                        }

                        var ifRefNotNull = block.IfRefNotNull(refname);
                        {
                            ifRefNotNull.AssignRef("hasNonNullRow", ConstantTrue());
                            var ifLeftNotNull = ifRefNotNull.IfCondition(NotEqualsNull(Ref("valueLeft")));
                            ifLeftNotNull.IfCondition(
                                    NotOptional(
                                        isAll,
                                        forge.Computer.Codegen(
                                            Ref("valueLeft"),
                                            valueLeftType,
                                            Ref(refname),
                                            typeof(object))))
                                .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                        }
                    }
                }
            }

            block.IfCondition(Not(Ref("hasNonNullRow")))
                .BlockReturn(ConstantNull());
            if (!valueLeftType.IsPrimitive) {
                block.IfRefNullReturnNull("valueLeft");
            }

            block.MethodReturn(Constant(isAll));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace