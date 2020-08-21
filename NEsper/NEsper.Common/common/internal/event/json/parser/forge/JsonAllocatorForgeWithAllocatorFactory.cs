///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonAllocatorForgeWithAllocatorFactory : JsonAllocatorForge
    {
        private readonly string _allocatorFactoryClassName;

        public JsonAllocatorForgeWithAllocatorFactory(string allocatorFactoryClassName)
        {
            _allocatorFactoryClassName = allocatorFactoryClassName;
        }

        public CodegenExpression NewDelegate(
            JsonDelegateRefs fields,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(JsonDeserializerBase), typeof(JsonForgeFactoryEventTypeTyped), classScope);
            method.Block
                .DeclareVar<JsonSerializationContext>("factory", NewInstanceInner(_allocatorFactoryClassName))
                .MethodReturn(ExprDotMethod(Ref("factory"), "make", fields.BaseHandler, fields.This));
            return LocalMethod(method);
        }
    }
} // end of namespace