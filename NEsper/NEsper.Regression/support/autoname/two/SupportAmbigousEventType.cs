namespace com.espertech.esper.regressionlib.support.autoname.two
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportAmbigousEventType
    {
        public SupportAmbigousEventType(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public void SetId(int id)
        {
            Id = id;
        }
    }
} // end of namespace