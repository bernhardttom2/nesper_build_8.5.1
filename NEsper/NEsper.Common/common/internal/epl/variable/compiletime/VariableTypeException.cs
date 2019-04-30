///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    /// <summary>
    /// Exception indicating a variable type error.
    /// </summary>
    public class VariableTypeException : VariableDeclarationException
    {
        /// <summary>Ctor.</summary>
        /// <param name="msg">the exception message.</param>
        public VariableTypeException(String msg)
            : base(msg)
        {
        }
    }
} // End of namespace