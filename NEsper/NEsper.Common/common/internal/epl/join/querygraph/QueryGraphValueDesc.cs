namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class QueryGraphValueDesc
    {
        public QueryGraphValueDesc(
            string[] indexExprs,
            QueryGraphValueEntry entry)
        {
            IndexExprs = indexExprs;
            Entry = entry;
        }

        public string[] IndexExprs { get; }

        public QueryGraphValueEntry Entry { get; }
    }
} // end of namespace