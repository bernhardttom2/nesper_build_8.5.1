///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants to match using the equals (=) operator.
    ///     The implementation is based on a regular HashMap.
    /// </summary>
    public class FilterParamIndexNotEqualsIs : FilterParamIndexNotEqualsBase
    {
        public FilterParamIndexNotEqualsIs(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock)
            : base(lookupable, readWriteLock, FilterOperator.IS_NOT)
        {
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches)
        {
            var attributeValue = Lookupable.Getter.Get(theEvent);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, attributeValue);
            }

            // Look up in hashtable
            using (ConstantsMapRwLock.ReadLock.Acquire()) {
                foreach (var entry in ConstantsMap) {
                    if (entry.Key == null) {
                        if (attributeValue != null) {
                            entry.Value.MatchEvent(theEvent, matches);
                        }

                        continue;
                    }

                    if (!entry.Key.Equals(attributeValue)) {
                        entry.Value.MatchEvent(theEvent, matches);
                    }
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterReverseIndex(null);
            }
        }
    }
} // end of namespace