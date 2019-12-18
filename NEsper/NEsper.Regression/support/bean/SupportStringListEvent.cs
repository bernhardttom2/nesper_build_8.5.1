///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportStringListEvent
    {
        public SupportStringListEvent(IList<string> myList)
        {
            MyNestedList = myList;
        }

        public IList<string> MyNestedList { get; }
    }
} // end of namespace