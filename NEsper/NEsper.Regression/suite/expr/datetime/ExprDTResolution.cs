///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTResolution
    {
        public static IList<RegressionExecution> Executions(bool isMicrosecond)
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTResolutionEventTime(isMicrosecond));
            executions.Add(new ExprDTLongProperty(isMicrosecond));
            return executions;
        }

        private static void RunAssertionLongProperty(
            RegressionEnvironment env,
            long startTime,
            SupportDateTime @event,
            string select,
            string[] fields,
            object[] expected)
        {
            env.AdvanceTime(startTime);

            var epl = "@Name('s0') select " + select + " from SupportDateTime";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(@event);
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.UndeployAll();
        }

        private static void RunAssertionEventTime(
            RegressionEnvironment env,
            long tsB,
            long flipTimeEndtsA)
        {
            env.AdvanceTime(0);
            var epl =
                "@Name('s0') select * from MyEvent(Id='A') as a unidirectional, MyEvent(Id='B')#lastevent as b where a.withDate(2002, 4, 30).before(b)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] {"B", tsB, tsB}, "MyEvent");

            env.SendEventObjectArray(new object[] {"A", flipTimeEndtsA - 1, flipTimeEndtsA - 1}, "MyEvent");
            Assert.IsTrue(env.Listener("s0").IsInvokedAndReset());

            env.SendEventObjectArray(new object[] {"A", flipTimeEndtsA, flipTimeEndtsA}, "MyEvent");
            Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

            env.UndeployAll();
        }

        public class ExprDTResolutionEventTime : RegressionExecution
        {
            private readonly bool isMicrosecond;

            public ExprDTResolutionEventTime(bool isMicrosecond)
            {
                this.isMicrosecond = isMicrosecond;
            }

            public void Run(RegressionEnvironment env)
            {
                var time = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
                if (!isMicrosecond) {
                    RunAssertionEventTime(env, time, time);
                }
                else {
                    RunAssertionEventTime(env, time * 1000, time * 1000);
                }
            }
        }

        internal class ExprDTLongProperty : RegressionExecution
        {
            private readonly bool isMicrosecond;

            public ExprDTLongProperty(bool isMicrosecond)
            {
                this.isMicrosecond = isMicrosecond;
            }

            public void Run(RegressionEnvironment env)
            {
                var time = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:05:06.007");
                var dtxTime = DateTimeEx.GetInstance(TimeZoneInfo.Local, time);

                var dtxMod = DateTimeEx.GetInstance(TimeZoneInfo.Local, time)
                    .SetHour(1)
                    .SetMinute(2)
                    .SetSecond(3)
                    .SetMillis(4);

                var select =
                    "LongDate.withTime(1, 2, 3, 4) as c0," +
                    "LongDate.set('hour', 1).set('minute', 2).set('second', 3).set('millisecond', 4).toCalendar() as c1," +
                    "LongDate.get('month') as c2," +
                    "current_timestamp.get('month') as c3," +
                    "current_timestamp.getMinuteOfHour() as c4," +
                    "current_timestamp.toDate() as c5," +
                    "current_timestamp.toCalendar() as c6," +
                    "current_timestamp.minus(1) as c7";
                var fields = new [] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7" };

                if (!isMicrosecond) {
                    RunAssertionLongProperty(
                        env,
                        time,
                        new SupportDateTime(time, null, null),
                        select,
                        fields,
                        new object[] {
                            dtxMod.TimeInMillis,
                            dtxMod, 4, 4, 5,
                            dtxTime.DateTime,
                            dtxTime,
                            time - 1
                        });
                }
                else {
                    RunAssertionLongProperty(
                        env,
                        time * 1000,
                        new SupportDateTime(time * 1000 + 123, null, null),
                        select,
                        fields,
                        new object[] {
                            dtxMod.TimeInMillis * 1000 + 123,
                            dtxMod, 4, 4, 5,
                            dtxTime.DateTime,
                            dtxTime,
                            time * 1000 - 1000
                        });
                }
            }
        }
    }
} // end of namespace