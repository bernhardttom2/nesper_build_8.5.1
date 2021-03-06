///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.util
{
    public class MutableBoolean
    {
        private bool value;

        public bool IsValue()
        {
            return value;
        }

        public void SetValue(bool value)
        {
            this.value = value;
        }
    }
} // end of namespace