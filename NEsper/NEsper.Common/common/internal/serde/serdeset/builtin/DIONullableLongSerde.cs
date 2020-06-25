///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	/// <summary>
	/// Binding for nullable long values.
	/// </summary>
	public class DIONullableLongSerde : DataInputOutputSerde<long?> {
	    public readonly static DIONullableLongSerde INSTANCE = new DIONullableLongSerde();

	    private DIONullableLongSerde() {
	    }

	    public void Write(long? @object, DataOutput output, byte[] pageFullKey, EventBeanCollatedWriter writer) {
	        Write(@object, output);
	    }

	    public void Write(long? @object, DataOutput stream) {
	        bool isNull = @object == null;
	        stream.WriteBoolean(isNull);
	        if (!isNull) {
	            stream.WriteLong(@object.Value);
	        }
	    }

	    public long? Read(DataInput input) {
	        return ReadInternal(input);
	    }

	    public long? Read(DataInput input, byte[] resourceKey) {
	        return ReadInternal(input);
	    }

	    private long? ReadInternal(DataInput s) {
	        bool isNull = s.ReadBoolean();
	        if (isNull) {
	            return null;
	        }
	        return s.ReadLong();
	    }
	}
} // end of namespace
