///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Factory for creating an event bean instance by writing property values to an underlying object.
    /// </summary>
    public interface EventBeanManufacturerForge
    {
        EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory);

        CodegenExpression Make(
            CodegenBlock codegenBlock,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace