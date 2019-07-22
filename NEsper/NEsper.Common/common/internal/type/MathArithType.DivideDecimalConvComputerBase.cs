///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public abstract class DivideDecimalConvComputerBase : Computer
        {
            private readonly SimpleNumberCoercer convOne;
            private readonly SimpleNumberCoercer convTwo;
            private readonly bool divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDecimalConvComputerBase(
                SimpleNumberCoercer convOne,
                SimpleNumberCoercer convTwo,
                bool divisionByZeroReturnsNull)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(
                object d1,
                object d2)
            {
                decimal s1 = convOne.CoerceBoxed(d1).AsDecimal();
                decimal s2 = convTwo.CoerceBoxed(d2).AsDecimal();
                if (s2 == 0.0m) {
                    if (divisionByZeroReturnsNull) {
                        return null;
                    }

                    var result = s1.AsDouble() / 0;
                    return new decimal(result);
                }

                return DoDivide(s1, s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                var block = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(DivideDecimalConvComputerBase), codegenClassScope)
                    .AddParam(ltype, "d1")
                    .AddParam(rtype, "d2")
                    .Block
                    .DeclareVar<decimal>(
                        "s1",
                        convOne.CoerceCodegen(CodegenExpressionBuilder.Ref("d1"), ltype))
                    .DeclareVar<decimal>(
                        "s2",
                        convTwo.CoerceCodegen(CodegenExpressionBuilder.Ref("d2"), rtype));
                var ifZeroDivisor =
                    block.IfCondition(
                        CodegenExpressionBuilder.EqualsIdentity(
                            CodegenExpressionBuilder.Ref("s2"),
                            CodegenExpressionBuilder.Constant(0.0m)));
                if (divisionByZeroReturnsNull) {
                    ifZeroDivisor.BlockReturn(CodegenExpressionBuilder.ConstantNull());
                }
                else {
                    ifZeroDivisor.DeclareVar<double>(
                            "result",
                            CodegenExpressionBuilder.Op(
                                CodegenExpressionBuilder.Ref("s1"),
                                "/",
                                CodegenExpressionBuilder.Constant(0.0m)))
                        .BlockReturn(
                            CodegenExpressionBuilder.NewInstance(
                                typeof(decimal?),
                                CodegenExpressionBuilder.Ref("result")));
                }

                var method = block.MethodReturn(
                    DoDivideCodegen(
                        CodegenExpressionBuilder.Ref("s1"),
                        CodegenExpressionBuilder.Ref("s2"),
                        codegenClassScope));
                return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }

            public abstract object DoDivide(
                decimal s1,
                decimal s2);

            public abstract CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1,
                CodegenExpressionRef s2,
                CodegenClassScope codegenClassScope);
        }
    }
}