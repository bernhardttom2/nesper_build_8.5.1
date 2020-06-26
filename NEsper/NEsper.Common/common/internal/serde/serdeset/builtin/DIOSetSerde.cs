///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIOSetSerde : DataInputOutputSerde<ISet<object>>
	{
		private readonly DataInputOutputSerde<object> inner;

		public DIOSetSerde(DataInputOutputSerde<object> inner)
		{
			this.inner = inner;
		}

		public void Write(
			ISet<object> set,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			output.WriteInt(set.Count);
			foreach (object @object in set) {
				inner.Write(@object, output, unitKey, writer);
			}
		}

		public ISet<object> Read(
			DataInput input,
			byte[] unitKey)
		{
			var size = input.ReadInt();
			var set = new HashSet<object>();
			for (int i = 0; i < size; i++) {
				set.Add(inner.Read(input, unitKey));
			}

			return set;
		}
	}
} // end of namespace
