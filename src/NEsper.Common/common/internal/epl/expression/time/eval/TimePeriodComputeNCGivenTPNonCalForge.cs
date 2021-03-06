///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.node;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeNCGivenTPNonCalForge : TimePeriodComputeForge
    {
        private readonly ExprTimePeriodForge timePeriodForge;

        public TimePeriodComputeNCGivenTPNonCalForge(ExprTimePeriodForge timePeriodForge)
        {
            this.timePeriodForge = timePeriodForge;
        }

        public TimePeriodCompute Evaluator {
            get => new TimePeriodComputeNCGivenTPNonCalEval(
                timePeriodForge.Evaluators,
                timePeriodForge.Adders,
                timePeriodForge.TimeAbacus);
        }

        public CodegenExpression MakeEvaluator(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(TimePeriodComputeNCGivenTPNonCalEval),
                this.GetType(),
                classScope);

            method.Block.DeclareVar<TimePeriodComputeNCGivenTPNonCalEval>(
                "eval",
                NewInstance(typeof(TimePeriodComputeNCGivenTPNonCalEval)));
            method.Block.SetProperty(
                Ref("eval"),
                "Adders",
                TimePeriodAdderUtil.MakeArray(timePeriodForge.Adders, parent, classScope));
            method.Block.SetProperty(
                Ref("eval"),
                "Evaluators",
                ExprNodeUtilityCodegen.CodegenEvaluators(
                    timePeriodForge.Forges,
                    method,
                    this.GetType(),
                    classScope));
            method.Block.SetProperty(
                Ref("eval"),
                "TimeAbacus",
                classScope.AddOrGetDefaultFieldSharable(TimeAbacusField.INSTANCE));
            method.Block.MethodReturn(Ref("eval"));

            return LocalMethod(method);
        }
    }
} // end of namespace