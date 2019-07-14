///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeTimeControlClockType
    {
        public void Run(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Common.AddEventType(typeof(SupportBean));
            var runtime = EPRuntimeProvider.GetRuntime(typeof(ClientRuntimeTimeControlClockType).Name, configuration);

            runtime.EventService.AdvanceTime(0);
            Assert.AreEqual(0, runtime.EventService.CurrentTime);
            Assert.IsTrue(runtime.EventService.IsExternalClockingEnabled());

            runtime.EventService.ClockInternal();
            Assert.IsFalse(runtime.EventService.IsExternalClockingEnabled());
            var waitStart = PerformanceObserver.MilliTime;
            while (PerformanceObserver.MilliTime - waitStart < 10000) {
                if (runtime.EventService.CurrentTime > 0) {
                    break;
                }
            }

            Assert.AreNotEqual(0, runtime.EventService.CurrentTime);
            Assert.IsTrue(PerformanceObserver.MilliTime > runtime.EventService.CurrentTime - 10000);

            runtime.EventService.ClockExternal();
            Assert.IsTrue(runtime.EventService.IsExternalClockingEnabled());
            runtime.EventService.AdvanceTime(0);
            ThreadSleep(500);
            Assert.AreEqual(0, runtime.EventService.CurrentTime);

            runtime.Destroy();
        }
    }
} // end of namespace