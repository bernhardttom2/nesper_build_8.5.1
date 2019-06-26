///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    /// Obtains the write lock of the runtime-wide event processing read-write lock by simply blocking until the lock was obtained.
    /// </summary>
    public class LockStrategyDefault : LockStrategy
    {
        /// <summary>
        /// The instance of the default lock strategy.
        /// </summary>
        public static readonly LockStrategyDefault INSTANCE = new LockStrategyDefault();

        private LockStrategyDefault()
        {
        }

        public void Acquire(ManagedReadWriteLock runtimeWideLock)
        {
            runtimeWideLock.AcquireWriteLock();
        }

        public void Release(ManagedReadWriteLock runtimeWideLock)
        {
            runtimeWideLock.ReleaseWriteLock();
        }
    }
} // end of namespace