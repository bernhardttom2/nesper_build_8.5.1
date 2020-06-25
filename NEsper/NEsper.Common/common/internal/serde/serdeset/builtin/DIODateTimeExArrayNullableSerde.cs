///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	public class DIODateTimeExArrayNullableSerde : DataInputOutputSerde<DateTimeEx[]>
	{
		public readonly static DIODateTimeExArrayNullableSerde INSTANCE = new DIODateTimeExArrayNullableSerde();

		private DIODateTimeExArrayNullableSerde()
		{
		}

		public void Write(
			DateTimeEx[] @object,
			DataOutput output)
		{
			WriteInternal(@object, output);
		}

		public DateTimeEx[] Read(DataInput input)
		{
			return ReadInternal(input);
		}

		public void Write(
			DateTimeEx[] @object,
			DataOutput output,
			byte[] unitKey,
			EventBeanCollatedWriter writer)
		{
			WriteInternal(@object, output);
		}

		public DateTimeEx[] Read(
			DataInput input,
			byte[] unitKey)
		{
			return ReadInternal(input);
		}

		private void WriteInternal(
			DateTimeEx[] @object,
			DataOutput output)
		{
			if (@object == null) {
				output.WriteInt(-1);
				return;
			}

			output.WriteInt(@object.Length);
			foreach (DateTimeEx value in @object) {
				DIODateTimeExSerde.INSTANCE.Write(value, output);
			}
		}

		private DateTimeEx[] ReadInternal(DataInput input)
		{
			int len = input.ReadInt();
			if (len == -1) {
				return null;
			}

			DateTimeEx[] array = new DateTimeEx[len];
			for (int i = 0; i < len; i++) {
				array[i] = DIODateTimeExSerde.INSTANCE.Read(input);
			}

			return array;
		}
	}
} // end of namespace
