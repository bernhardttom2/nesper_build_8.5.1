///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoin6Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With0(execs);
            With1(execs);
            With2(execs);
            With3(execs);
            With4(execs);
            With5(execs);
            return execs;
        }

        public static IList<RegressionExecution> With5(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS5());
            return execs;
        }

        public static IList<RegressionExecution> With4(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS4());
            return execs;
        }

        public static IList<RegressionExecution> With3(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS3());
            return execs;
        }

        public static IList<RegressionExecution> With2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS2());
            return execs;
        }

        public static IList<RegressionExecution> With1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS1());
            return execs;
        }

        public static IList<RegressionExecution> With0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS0());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            object[] s0Events;
            object[] s1Events;
            object[] s2Events;
            object[] s3Events;
            object[] s4Events;
            object[] s5Events;

            // Test s0 and s1=0, s2=0, s3=0, s4=0, s5=0
            //
            s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=1, s2=0, s3=0, s4=0, s5=0
            //
            s1Events = SupportBean_S1.MakeS1("B", new[] {"B-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=1, s2=1, s3=0, s4=0, s5=0
            //
            s1Events = SupportBean_S1.MakeS1("C", new[] {"C-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("C", new[] {"C-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=1, s2=1, s3=1, s4=0, s5=0
            //
            s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("D", new[] {"D-s2-1"});
            SendEvent(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("D", new[] {"D-s2-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=1, s2=1, s3=1, s4=1, s5=0
            //
            s1Events = SupportBean_S1.MakeS1("E", new[] {"E-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("E", new[] {"E-s2-1"});
            SendEvent(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("E", new[] {"E-s2-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            s4Events = SupportBean_S4.MakeS4("E", new[] {"E-s2-1"});
            SendEvent(env, s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, null, s4Events[0], null}
                },
                GetAndResetNewEvents(env));

            s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=2, s2=1, s3=1, s4=1, s5=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new[] {"F-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("F", new[] {"F-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("F", new[] {"F-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            s4Events = SupportBean_S4.MakeS4("F", new[] {"F-s2-1"});
            SendEvent(env, s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, null, s4Events[0], null}
                },
                GetAndResetNewEvents(env));

            s5Events = SupportBean_S5.MakeS5("F", new[] {"F-s2-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=1, s4=1, s5=2
            //
            s1Events = SupportBean_S1.MakeS1("G", new[] {"G-s1-1", "G-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("G", new[] {"G-s2-1", "G-s2-2"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("G", new[] {"G-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("G", new[] {"G-s2-1"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("G", new[] {"G-s5-1", "G-s5-2"});
            SendEventsAndReset(env, s5Events);

            s0Events = SupportBean_S0.MakeS0("G", new[] {"G-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=2, s4=2, s5=2
            //
            s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-1", "H-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("H", new[] {"H-s2-1", "H-s2-2"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("H", new[] {"H-s3-1", "H-s3-2"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("H", new[] {"H-s4-1", "H-s4-2"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("H", new[] {"H-s5-1", "H-s5-2"});
            SendEventsAndReset(env, s5Events);

            s0Events = SupportBean_S0.MakeS0("H", new[] {"H-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1], s5Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=1, s3=1, s4=3, s5=1
            //
            s1Events = SupportBean_S1.MakeS1("I", new[] {"I-s1-1", "I-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("I", new[] {"I-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("I", new[] {"I-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("I", new[] {"I-s4-1", "I-s4-2", "I-s4-3"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("I", new[] {"I-s5-1"});
            SendEventsAndReset(env, s5Events);

            s0Events = SupportBean_S0.MakeS0("I", new[] {"I-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[2], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[2], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s3=0
            //
            s1Events = SupportBean_S1.MakeS1("J", new[] {"J-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s1 and s0=1, s2=0, s3=1, s4=1, s5=0
            //
            s0Events = SupportBean_S0.MakeS0("K", new[] {"K-s0-1"});
            SendEvent(env, s0Events);

            s3Events = SupportBean_S3.MakeS3("K", new[] {"K-s3-1"});
            SendEventsAndReset(env, s3Events);

            s1Events = SupportBean_S1.MakeS1("K", new[] {"K-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1
            //
            s0Events = SupportBean_S0.MakeS0("L", new[] {"L-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("L", new[] {"L-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("L", new[] {"L-s3-1"});
            SendEventsAndReset(env, s3Events);

            s5Events = SupportBean_S5.MakeS5("L", new[] {"L-s5-1"});
            SendEventsAndReset(env, s5Events);

            s1Events = SupportBean_S1.MakeS1("L", new[] {"L-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s0=1, s2=1, s3=1, s4=2, s5=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new[] {"M-s0-1"});
            SendEvent(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("M", new[] {"M-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("M", new[] {"M-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("M", new[] {"M-s4-1", "M-s4-2"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("M", new[] {"M-s5-1"});
            SendEventsAndReset(env, s5Events);

            s1Events = SupportBean_S1.MakeS1("M", new[] {"M-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=1, s1=0, s3=0, s4=1, s5=2
            //
            s0Events = SupportBean_S0.MakeS0("N", new[] {"N-s0-1"});
            SendEvent(env, s0Events);

            s4Events = SupportBean_S4.MakeS4("N", new[] {"N-s4-1"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("N", new[] {"N-s5-1", "N-s5-2"});
            SendEventsAndReset(env, s5Events);

            s2Events = SupportBean_S2.MakeS2("N", new[] {"N-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]},
                    new[] {null, null, s2Events[0], null, s4Events[0], s5Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=1, s1=1, s3=3, s4=1, s5=2
            //
            s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1"});
            SendEvent(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("O", new[] {"O-s3-1", "O-s3-2", "O-s3-3"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("O", new[] {"O-s4-1"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("O", new[] {"O-s5-1", "O-s5-2"});
            SendEventsAndReset(env, s5Events);

            s2Events = SupportBean_S2.MakeS2("O", new[] {"O-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2], s4Events[0], s5Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=0, s1=0, s2=0, s4=0, s5=0
            //
            s3Events = SupportBean_S3.MakeS3("P", new[] {"P-s1-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=0, s1=1, s2=0, s4=0, s5=0
            //
            s1Events = SupportBean_S1.MakeS1("Q", new[] {"Q-s1-1"});
            SendEvent(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("Q", new[] {"Q-s1-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=2, s2=2, s4=0, s5=0
            //
            s0Events = SupportBean_S0.MakeS0("R", new[] {"R-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("R", new[] {"R-s1-1", "R-s1-2"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("R", new[] {"R-s2-1", "R-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("R", new[] {"R-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, s1Events[0], null, s3Events[0], null, null},
                    new[] {null, s1Events[1], null, s3Events[0], null, null}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=2, s1=2, s2=1, s4=2, s5=2
            //
            s0Events = SupportBean_S0.MakeS0("S", new[] {"S-s0-1", "S-s0-2"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("S", new[] {"S-s1-1", "S-s1-2"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("S", new[] {"S-s2-1", "S-s2-1"});
            SendEventsAndReset(env, s2Events);

            s4Events = SupportBean_S4.MakeS4("S", new[] {"S-s4-1", "S-s4-2"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("S", new[] {"S-s5-1", "S-s5-2"});
            SendEventsAndReset(env, s5Events);

            s3Events = SupportBean_S3.MakeS3("S", new[] {"s-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
                    new[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s4 and s0=1, s1=0, s2=1, s3=0, s5=0
            //
            s0Events = SupportBean_S0.MakeS0("U", new[] {"U-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("U", new[] {"U-s1-1"});
            SendEventsAndReset(env, s2Events);

            s4Events = SupportBean_S4.MakeS4("U", new[] {"U-s4-1"});
            SendEvent(env, s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, null, s4Events[0], null}
                },
                GetAndResetNewEvents(env));

            // Test s4 and s0=1, s1=0, s2=1, s3=0, s5=1
            //
            s0Events = SupportBean_S0.MakeS0("V", new[] {"V-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("V", new[] {"V-s1-1"});
            SendEventsAndReset(env, s2Events);

            s5Events = SupportBean_S5.MakeS5("V", new[] {"V-s5-1"});
            SendEventsAndReset(env, s5Events);

            s4Events = SupportBean_S4.MakeS4("V", new[] {"V-s4-1"});
            SendEvent(env, s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s4 and s0=1, s1=1, s2=1, s3=1, s5=2
            //
            s0Events = SupportBean_S0.MakeS0("W", new[] {"W-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("W", new[] {"W-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("W", new[] {"W-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("W", new[] {"W-s3-1"});
            SendEventsAndReset(env, s3Events);

            s5Events = SupportBean_S5.MakeS5("W", new[] {"W-s5-1", "W-s5-2"});
            SendEventsAndReset(env, s5Events);

            s4Events = SupportBean_S4.MakeS4("W", new[] {"W-s4-1"});
            SendEvent(env, s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s5 and s0=1, s1=2, s2=2, s3=1, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("X", new[] {"X-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("X", new[] {"X-s1-1", "X-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("X", new[] {"X-s2-1", "X-s2-2"});
            SendEvent(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("X", new[] {"X-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("X", new[] {"X-s4-1"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("X", new[] {"X-s5-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s5 and s0=2, s1=1, s2=1, s3=1, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("Y", new[] {"Y-s0-1", "Y-s0-2"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("Y", new[] {"Y-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("Y", new[] {"Y-s2-1"});
            SendEvent(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("Y", new[] {"Y-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("Y", new[] {"Y-s4-1"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("Y", new[] {"X-s5-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s5 and s0=1, s1=1, s2=1, s3=2, s4=2
            //
            s0Events = SupportBean_S0.MakeS0("Z", new[] {"Z-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("Z", new[] {"Z-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("Z", new[] {"Z-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("Z", new[] {"Z-s3-1", "Z-s3-2"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("Z", new[] {"Z-s4-1", "Z-s4-2"});
            SendEventsAndReset(env, s4Events);

            s5Events = SupportBean_S5.MakeS5("Z", new[] {"Z-s5-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[0]}
                },
                GetAndResetNewEvents(env));

            env.UndeployAll();
        }

        private static void SendEventsAndReset(
            RegressionEnvironment env,
            object[] events)
        {
            SendEvent(env, events);
            env.Listener("s0").Reset();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        private static object[][] GetAndResetNewEvents(RegressionEnvironment env)
        {
            var newEvents = env.Listener("s0").LastNewData;
            env.Listener("s0").Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[] {"S0", "S1", "S2", "S3", "S4", "S5"});
        }

        internal class EPLJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " right outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " right outer join SupportBean_S4#length(1000) as S4 on S2.P20 = S4.P40 " +
                          " right outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S1#length(1000) as S1 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S4#length(1000) as S4 on S2.P20 = S4.P40 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S2#length(1000) as S2 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " right outer join SupportBean_S4#length(1000) as S4 on S2.P20 = S4.P40 " +
                          " right outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S3#length(1000) as S3 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S1.P10 = S3.P30 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S4#length(1000) as S4 on S2.P20 = S4.P40 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS4 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S4#length(1000) as S4 " +
                          " left outer join SupportBean_S2#length(1000) as S2 on S2.P20 = S4.P40 " +
                          " right outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS5 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S5#length(1000) as S5 " +
                          " left outer join SupportBean_S2#length(1000) as S2 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S4#length(1000) as S4 on S2.P20 = S4.P40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }
    }
} // end of namespace