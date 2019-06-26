///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterIndexedRuntimeKeyed : EventPropertyGetterIndexedSPI
    {
        private readonly Field _pos;

        public AvroEventBeanGetterIndexedRuntimeKeyed(Field pos)
        {
            _pos = pos;
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = (ICollection<object>) record.Get(_pos);
            return AvroEventBeanGetterIndexed.GetAvroIndexedValue(values, index);
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            var method = codegenMethodScope.MakeChild(typeof(object), typeof(AvroEventBeanGetterIndexedRuntimeKeyed), codegenClassScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(int), "index").Block
                .DeclareVar(typeof(GenericRecord), "record", CodegenExpressionBuilder.CastUnderlying(typeof(GenericRecord), CodegenExpressionBuilder.Ref("event")))
                .DeclareVar(
                    typeof(ICollection<object>), "values", CodegenExpressionBuilder.Cast(typeof(ICollection<object>), CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("record"), "get", CodegenExpressionBuilder.Constant(_pos))))
                .MethodReturn(CodegenExpressionBuilder.StaticMethod(typeof(AvroEventBeanGetterIndexed), "getAvroIndexedValue", CodegenExpressionBuilder.Ref("values"), CodegenExpressionBuilder.Ref("index")));
            return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(beanExpression).Pass(key).Call();
        }
    }
} // end of namespace