///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    public interface TypeWidenerCustomizer
    {
        TypeWidenerSPI WidenerFor(
            string columnName,
            Type columnType,
            Type writeablePropertyType,
            string writeablePropertyName,
            string statementName);
    }
} // end of namespace