///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.json.deserializers.array;
using com.espertech.esper.common.@internal.@event.json.parser.core;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array2dim
{
    public class JsonDeserializerArray2DPrimitiveSingle : JsonDeserializerArrayBase<float[]>
    {
        public JsonDeserializerArray2DPrimitiveSingle()
            : base(_ => _.ElementToArray(v => v.GetSmartSingle()))
        {
        }
    }
} // end of namespace
