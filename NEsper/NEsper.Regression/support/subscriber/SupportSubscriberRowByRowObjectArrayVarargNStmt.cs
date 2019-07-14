namespace com.espertech.esper.regressionlib.support.subscriber
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportSubscriberRowByRowObjectArrayVarargNStmt : SupportSubscriberRowByRowObjectArrayBase
    {
        public SupportSubscriberRowByRowObjectArrayVarargNStmt() : base(false)
        {
        }

        public void Update(params object[] row)
        {
            AddIndication(row);
        }
    }
} // end of namespace