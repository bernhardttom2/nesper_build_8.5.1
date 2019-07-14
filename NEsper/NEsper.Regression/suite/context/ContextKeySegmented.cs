///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmented
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedPatternFilter());
            execs.Add(new ContextKeySegmentedJoinRemoveStream());
            execs.Add(new ContextKeySegmentedSelector());
            execs.Add(new ContextKeySegmentedLargeNumberPartitions());
            execs.Add(new ContextKeySegmentedAdditionalFilters());
            execs.Add(new ContextKeySegmentedMultiStatementFilterCount());
            execs.Add(new ContextKeySegmentedSubtype());
            execs.Add(new ContextKeySegmentedJoinMultitypeMultifield());
            execs.Add(new ContextKeySegmentedSubselectPrevPrior());
            execs.Add(new ContextKeySegmentedPrior());
            execs.Add(new ContextKeySegmentedSubqueryFiltered());
            execs.Add(new ContextKeySegmentedJoin());
            execs.Add(new ContextKeySegmentedPattern());
            execs.Add(new ContextKeySegmentedPatternSceneTwo());
            execs.Add(new ContextKeySegmentedViewSceneOne());
            execs.Add(new ContextKeySegmentedViewSceneTwo());
            execs.Add(new ContextKeySegmentedJoinWhereClauseOnPartitionKey());
            execs.Add(new ContextKeySegmentedNullSingleKey());
            execs.Add(new ContextKeySegmentedNullKeyMultiKey());
            execs.Add(new ContextKeySegmentedInvalid());
            execs.Add(new ContextKeySegmentedTermByFilter());
            execs.Add(new ContextKeySegmentedMatchRecognize());
            return execs;
        }

        private static void AssertViewData(
            RegressionEnvironment env,
            int newIntExpected,
            object[][] newArrayExpected,
            int? oldIntExpected)
        {
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
            Assert.AreEqual(newIntExpected, env.Listener("s0").LastNewData[0].Get("IntPrimitive"));
            var beans = (SupportBean[]) env.Listener("s0").LastNewData[0].Get("pw");
            Assert.AreEqual(newArrayExpected.Length, beans.Length);
            for (var i = 0; i < beans.Length; i++) {
                Assert.AreEqual(newArrayExpected[i][0], beans[i].TheString);
                Assert.AreEqual(newArrayExpected[i][1], beans[i].IntPrimitive);
            }

            if (oldIntExpected != null) {
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual(oldIntExpected, env.Listener("s0").LastOldData[0].Get("IntPrimitive"));
            }
            else {
                Assert.IsNull(env.Listener("s0").LastOldData);
            }

            env.Listener("s0").Reset();
        }

        private static void SendWebEventsIncomplete(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportWebEvent("Start", Convert.ToString(id)));
            env.SendEventBean(new SupportWebEvent("End", Convert.ToString(id)));
        }

        private static void SendWebEventsComplete(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportWebEvent("Start", Convert.ToString(id)));
            env.SendEventBean(new SupportWebEvent("Middle", Convert.ToString(id)));
            env.SendEventBean(new SupportWebEvent("End", Convert.ToString(id)));
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        public static bool StringContainsX(string theString)
        {
            return theString.Contains("X");
        }

        private static void SendAssertSB(
            long expected,
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "theString,cnt".SplitCsv(),
                new object[] {theString, expected});
        }

        private static void SendAssertNone(
            RegressionEnvironment env,
            object @event)
        {
            env.SendEventBean(@event);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendSBEvent(
            RegressionEnvironment env,
            string @string,
            int? intBoxed,
            int intPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }

        internal class ContextKeySegmentedPatternFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext = "create context IndividualBean partition by theString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var eplAnalysis = "@Name('s0') context IndividualBean " +
                                  "select * from pattern [every (event1=SupportBean(stringContainsX(TheString) = false) => event2=SupportBean(stringContainsX(TheString) = true))]";
                env.CompileDeploy(eplAnalysis, path).AddListener("s0");

                env.SendEventBean(new SupportBean("F1", 0));
                env.SendEventBean(new SupportBean("F1", 0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("X1", 0));
                Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

                env.Milestone(1);

                env.SendEventBean(new SupportBean("X1", 0));
                Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedMatchRecognize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContextOne = "create context SegmentedByString partition by theString from SupportBean";
                env.CompileDeploy(eplContextOne, path);

                var eplMatchRecog = "@Name('s0') context SegmentedByString " +
                                    "select * from SupportBean\n" +
                                    "match_recognize ( \n" +
                                    "  measures A.longPrimitive as a, B.longPrimitive as b\n" +
                                    "  pattern (A B) \n" +
                                    "  define " +
                                    "    A as A.IntPrimitive = 1," +
                                    "    B as B.IntPrimitive = 2\n" +
                                    ")";
                env.CompileDeploy(eplMatchRecog, path).AddListener("s0");

                env.SendEventBean(MakeEvent("A", 1, 10));

                env.Milestone(0);

                env.SendEventBean(MakeEvent("B", 1, 30));

                env.Milestone(1);

                env.SendEventBean(MakeEvent("A", 2, 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a,b".SplitCsv(),
                    new object[] {10L, 20L});

                env.Milestone(2);

                env.SendEventBean(MakeEvent("B", 2, 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a,b".SplitCsv(),
                    new object[] {30L, 40L});

                env.UndeployAll();

                // try with "prev"
                path.Clear();
                var eplContextTwo = "create context SegmentedByString partition by theString from SupportBean";
                env.CompileDeploy(eplContextTwo, path);

                var eplMatchRecogWithPrev = "@Name('s0') context SegmentedByString select * from SupportBean " +
                                            "match_recognize ( " +
                                            "  measures A.longPrimitive as e1, B.longPrimitive as e2" +
                                            "  pattern (A B) " +
                                            "  define A as A.IntPrimitive >= prev(A.IntPrimitive),B as B.IntPrimitive >= prev(B.IntPrimitive) " +
                                            ")";
                env.CompileDeploy(eplMatchRecogWithPrev, path).AddListener("s0");

                env.SendEventBean(MakeEvent("A", 1, 101));
                env.SendEventBean(MakeEvent("B", 1, 201));

                env.Milestone(1);

                env.SendEventBean(MakeEvent("A", 2, 102));
                env.SendEventBean(MakeEvent("B", 2, 202));

                env.Milestone(2);

                env.SendEventBean(MakeEvent("A", 3, 103));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "e1,e2".SplitCsv(),
                    new object[] {102L, 103L});

                env.Milestone(3);

                env.SendEventBean(MakeEvent("B", 3, 203));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "e1,e2".SplitCsv(),
                    new object[] {202L, 203L});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoinRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();

                var stmtContext = "create context SegmentedBySession partition by sessionId from SupportWebEvent";
                env.CompileDeploy(stmtContext, path);

                var epl = "@Name('s0') context SegmentedBySession " +
                          " select rstream A.pageName as pageNameA , A.sessionId as sessionIdA, B.pageName as pageNameB, C.pageName as pageNameC from " +
                          "SupportWebEvent(pageName='Start')#time(30) A " +
                          "full outer join " +
                          "SupportWebEvent(pageName='Middle')#time(30) B on A.sessionId = B.sessionId " +
                          "full outer join " +
                          "SupportWebEvent(pageName='End')#time(30) C on A.sessionId  = C.sessionId " +
                          "where A.pageName is not null and (B.pageName is null or C.pageName is null) ";
                env.CompileDeploy(epl, path);

                env.AddListener("s0");

                // Set up statement for finding missing events
                SendWebEventsComplete(env, 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(20000);
                SendWebEventsComplete(env, 1);

                env.AdvanceTime(40000);
                SendWebEventsComplete(env, 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(60000);
                SendWebEventsIncomplete(env, 3);

                env.AdvanceTime(80000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(100000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSelector : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context PartitionedByString partition by theString from SupportBean", path);
                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context PartitionedByString select context.key1 as c0, sum(IntPrimitive) as c1 from SupportBean#length(5)",
                    path);

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E2", 21));

                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10}, new object[] {"E2", 41}});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10}, new object[] {"E2", 41}});

                // test iterator targeted
                var selector = new SupportSelectorPartitioned(Collections.SingletonList(new object[] {"E2"}));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    new[] {new object[] {"E2", 41}});
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(new SupportSelectorPartitioned((IList<object[]>) null))
                        .MoveNext());
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(new SupportSelectorPartitioned(Collections.SingletonList(new object[] {"EX"})))
                        .MoveNext());
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(new SupportSelectorPartitioned(new EmptyList<object[]>()))
                        .MoveNext());

                // test iterator filtered
                var filtered = new MySelectorFilteredPartitioned(new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(filtered),
                    env.Statement("s0").GetSafeEnumerator(filtered),
                    fields,
                    new[] {new object[] {"E2", 41}});

                // test always-false filter - compare context partition info
                var filteredFalse = new MySelectorFilteredPartitioned(null);
                Assert.IsFalse(env.Statement("s0").GetEnumerator(filteredFalse).MoveNext());
                EPAssertionUtil.AssertEqualsAnyOrder(
                    new object[] {
                        new object[] {"E1"},
                        new object[] {"E2"}
                    },
                    filteredFalse.Contexts.ToArray());

                try {
                    env.Statement("s0")
                        .GetEnumerator(
                            new ProxyContextPartitionSelectorCategory {
                                ProcLabels = () => null
                            });
                    Assert.Fail();
                }
                catch (InvalidContextPartitionSelector ex) {
                    Assert.IsTrue(
                        ex.Message.StartsWith(
                            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com."),
                        "message: " + ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid filter spec
                epl = "create context SegmentedByAString partition by string from SupportBean(dummy = 1)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

                // property not found
                epl = "create context SegmentedByAString partition by dummy from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'SegmentedByAString' property name 'dummy' not found on type SupportBean [");

                // mismatch number pf properties
                epl =
                    "create context SegmentedByAString partition by theString from SupportBean, id, p00 from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'SegmentedByAString' expected the same number of property names for each event type, found 1 properties for event type 'SupportBean' and 2 properties for event type 'SupportBean_S0' [create context SegmentedByAString partition by theString from SupportBean, id, p00 from SupportBean_S0]");

                // incompatible property types
                epl =
                    "create context SegmentedByAString partition by theString from SupportBean, id from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'SegmentedByAString' for context 'SegmentedByAString' found mismatch of property types, property 'theString' of type 'System.String' compared to property 'id' of type 'System.Integer' [");

                // duplicate type specification
                epl =
                    "create context SegmentedByAString partition by theString from SupportBean, theString from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'SegmentedByAString' the event type 'SupportBean' is listed twice [");

                // duplicate type: subtype
                epl = "create context SegmentedByAString partition by baseAB from ISupportBaseAB, a from ISupportA";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'SegmentedByAString' the event type 'ISupportA' is listed twice: Event type 'ISupportA' is a subtype or supertype of event type 'ISupportBaseAB' [");

                // validate statement not applicable filters
                var path = new RegressionPath();
                env.CompileDeploy("create context SegmentedByAString partition by theString from SupportBean", path);
                epl = "context SegmentedByAString select * from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Segmented context 'SegmentedByAString' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

                // invalid attempt to partition a named window's streams
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);
                epl = "create context SegmentedByWhat partition by theString from MyWindow";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Partition criteria may not include named windows [create context SegmentedByWhat partition by theString from MyWindow]");

                // partitioned with named window
                env.CompileDeploy("create schema SomeSchema(ipAddress string)", path);
                env.CompileDeploy("create context TheSomeSchemaCtx Partition By ipAddress From SomeSchema", path);
                epl = "context TheSomeSchemaCtx create window MyEvent#time(30 sec) (ipAddress string)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Segmented context 'TheSomeSchemaCtx' requires that named windows are associated to an existing event type and that the event type is listed among the partitions defined by the create-context statement");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedLargeNumberPartitions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByAString  partition by theString from SupportBean",
                    path);

                var fields = "col1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByAString " +
                    "select sum(IntPrimitive) as col1," +
                    "prev(1, intPrimitive)," +
                    "prior(1, intPrimitive)," +
                    "(select id from SupportBean_S0#lastevent)" +
                    "  from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean("E" + i, i));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {i});
                }

                env.UndeployAll();
            }
        }

        public class ContextKeySegmentedAdditionalFilters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByAString " +
                    "partition by theString from SupportBean(intPrimitive>0), p00 from SupportBean_S0(id > 0)",
                    path);

                // first send a view events
                env.SendEventBean(new SupportBean("B1", -1));
                env.SendEventBean(new SupportBean_S0(-2, "S0"));
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                var fields = "col1,col2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByAString " +
                    "select sum(sb.IntPrimitive) as col1, sum(s0.id) as col2 " +
                    "from pattern [every (s0=SupportBean_S0 or sb=SupportBean)]",
                    path);
                env.AddListener("s0");

                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(-3, "S0"));
                env.SendEventBean(new SupportBean("S0", -1));
                env.SendEventBean(new SupportBean("S1", -2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(2, "S0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, 2});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("S1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, null});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(-2, "S0"));
                env.SendEventBean(new SupportBean("S1", -10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(3, "S1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 3});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("S0", 9));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {9, 2});

                env.Milestone(6);

                env.UndeployAll();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(7);

                // Test unnecessary filter
                var epl = "create context CtxSegmented partition by theString from SupportBean;" +
                          "context CtxSegmented select * from pattern [every a=SupportBean => c=SupportBean(c.TheString=a.TheString)];";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedMultiStatementFilterCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByAString " +
                    "partition by theString from SupportBean, p00 from SupportBean_S0",
                    path);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                // first send a view events
                env.SendEventBean(new SupportBean("B1", 1));
                env.SendEventBean(new SupportBean_S0(10, "S0"));

                string[] fields = {"col1"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByAString select sum(id) as col1 from SupportBean_S0",
                    path);
                env.AddListener("s0");

                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean_S0(10, "S0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});

                env.Milestone(0);

                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean_S0(8, "S1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {8});

                env.Milestone(1);

                Assert.AreEqual(4, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean_S0(4, "S0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {14});

                env.Milestone(2);

                Assert.AreEqual(4, SupportFilterHelper.GetFilterCountApprox(env));

                env.CompileDeploy(
                    "@Name('s1') context SegmentedByAString select sum(IntPrimitive) as col1 from SupportBean",
                    path);
                env.AddListener("s1");

                Assert.AreEqual(6, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("S0", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5});

                Assert.AreEqual(6, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("S2", 6));
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {6});

                Assert.AreEqual(8, SupportFilterHelper.GetFilterCountApprox(env));

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(
                    5,
                    SupportFilterHelper
                        .GetFilterCountApprox(env)); // 5 = 3 from context instances and 2 from context itself

                env.Milestone(4);

                env.UndeployModuleContaining("s1");
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                env.UndeployModuleContaining("context");
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubtype : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "col1".SplitCsv();
                var epl =
                    "@Name('context') create context SegmentedByString partition by baseAB from ISupportBaseAB;\n" +
                    "@Name('s0') context SegmentedByString select count(*) as col1 from ISupportA;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new ISupportAImpl("A1", "AB1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});

                env.SendEventBean(new ISupportAImpl("A2", "AB1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});

                env.Milestone(1);

                env.SendEventBean(new ISupportAImpl("A3", "AB2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});

                env.SendEventBean(new ISupportAImpl("A4", "AB1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3L});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoinMultitypeMultifield : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedBy2Fields " +
                    "partition by theString and intPrimitive from SupportBean, p00 and id from SupportBean_S0",
                    path);

                var fields = "c1,c2,c3,c4,c5,c6".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context SegmentedBy2Fields " +
                    "select TheString as c1, intPrimitive as c2, id as c3, p00 as c4, context.key1 as c5, context.key2 as c6 " +
                    "from SupportBean#lastevent, SupportBean_S0#lastevent",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2, "G1"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 2));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "G2"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 1, 1, "G2", "G2", 1});

                env.SendEventBean(new SupportBean_S0(2, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, 2, "G2", "G2", 2});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1, 1, "G1", "G1", 1});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 2, "G1", "G1", 2});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubselectPrevPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by theString from SupportBean",
                    path);

                string[] fieldsPrev = {"TheString", "col1"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select TheString, (select prev(0, id) from SupportBean_S0#keepall) as col1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrev,
                    new object[] {"G1", null});

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrev,
                    new object[] {"G1", 1});

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrev,
                    new object[] {"G2", null});

                env.SendEventBean(new SupportBean_S0(2, "E2"));
                env.SendEventBean(new SupportBean("G2", 21));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrev,
                    new object[] {"G2", 2});

                env.SendEventBean(new SupportBean("G1", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrev,
                    new object[] {"G1", null}); // since returning multiple rows

                env.UndeployModuleContaining("s0");

                string[] fieldsPrior = {"TheString", "col1"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select TheString, (select prior(0, id) from SupportBean_S0#keepall) as col1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"G1", null});

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"G1", 1});

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"G2", null}); // since category started as soon as statement added

                env.SendEventBean(new SupportBean_S0(2, "E2"));
                env.SendEventBean(new SupportBean("G2", 21));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"G2", 2}); // since returning multiple rows

                env.SendEventBean(new SupportBean("G1", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"G1", null}); // since returning multiple rows

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by theString from SupportBean",
                    path);

                string[] fields = {"val0", "val1"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select IntPrimitive as val0, prior(1, intPrimitive) as val1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, null});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20, null});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 10});

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubqueryFiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by theString from SupportBean",
                    path);

                string[] fields = {"TheString", "IntPrimitive", "val0"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select TheString, intPrimitive, (select p00 from SupportBean_S0#lastevent as s0 where sb.IntPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "s1"));
                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, null});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "s2"));
                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, "s2"});

                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 10, null});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10, "s3"));
                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 10, "s3"});

                env.SendEventBean(new SupportBean("G3", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 10, null});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, "s3"});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by theString from SupportBean",
                    path);

                string[] fields = {"sb.TheString", "sb.IntPrimitive", "s0.id"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select * from SupportBean#keepall as sb, SupportBean_S0#keepall as s0 " +
                    "where intPrimitive = id",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20, 20});

                env.SendEventBean(new SupportBean_S0(30));
                env.SendEventBean(new SupportBean("G3", 30));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G1", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 30, 30});

                env.SendEventBean(new SupportBean("G2", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 30, 30});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by theString from SupportBean",
                    path);

                string[] fields = {"a.TheString", "a.IntPrimitive", "b.TheString", "b.IntPrimitive"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean => b=SupportBean(intPrimitive=a.IntPrimitive+1)]",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G1", 20));
                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 21));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20, "G2", 21});

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, "G1", 11});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 22));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 21, "G2", 22});

                env.UndeployModuleContaining("s0");

                // add another statement: contexts already exist, this one uses @Consume
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean => b=SupportBean(intPrimitive=a.IntPrimitive+1)@Consume]",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G1", 20));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G2", 21));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20, "G2", 21});

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, "G1", 11});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G2", 22));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployModuleContaining("s0");

                // test truly segmented consume
                string[] fieldsThree = {"a.TheString", "a.IntPrimitive", "b.id", "b.p00"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean => b=SupportBean_S0(id=a.IntPrimitive)@Consume]",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(10, "E1")); // should be 2 output rows
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").LastNewData,
                    fieldsThree,
                    new[] {new object[] {"G1", 10, 10, "E1"}, new object[] {"G2", 10, 10, "E1"}});

                env.UndeployAll();
            }
        }

        public class ContextKeySegmentedPatternSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('CTX') create context SegmentedByString partition by theString from SupportBean, p00 from SupportBean_S0;\n" +
                    "@Name('S1') context SegmentedByString " +
                    "select a.TheString as c0, a.IntPrimitive as c1, b.id as c2, b.p00 as c3 from pattern [" +
                    "every a=SupportBean => b=SupportBean_S0(id=a.IntPrimitive)];\n";
                env.CompileDeploy(epl).AddListener("S1");
                var fields = "c0,c1,c2,c3".SplitCsv();

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                env.SendEventBean(new SupportBean_S0(0, "G1"));
                env.SendEventBean(new SupportBean_S0(10, "G2"));
                Assert.IsFalse(env.Listener("S1").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(20, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20, 20, "G2"});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(20, "G2"));
                env.SendEventBean(new SupportBean_S0(0, "G1"));
                Assert.IsFalse(env.Listener("S1").IsInvoked);

                env.SendEventBean(new SupportBean_S0(10, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, 10, "G1"});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedViewSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var contextEPL =
                    "@Name('context') create context SegmentedByString as partition by theString from SupportBean";
                env.CompileDeploy(contextEPL, path);

                var fieldsIterate = "IntPrimitive".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select irstream intPrimitive, prevwindow(items) as pw from SupportBean#length(2) as items",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                AssertViewData(
                    env,
                    10,
                    new[] {new object[] {"G1", 10}},
                    null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fieldsIterate,
                    new[] {new object[] {10}});

                env.SendEventBean(new SupportBean("G2", 20));
                AssertViewData(
                    env,
                    20,
                    new[] {new object[] {"G2", 20}},
                    null);

                env.SendEventBean(new SupportBean("G1", 11));
                AssertViewData(
                    env,
                    11,
                    new[] {new object[] {"G1", 11}, new object[] {"G1", 10}},
                    null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fieldsIterate,
                    new[] {new object[] {10}, new object[] {11}, new object[] {20}});

                env.SendEventBean(new SupportBean("G2", 21));
                AssertViewData(
                    env,
                    21,
                    new[] {new object[] {"G2", 21}, new object[] {"G2", 20}},
                    null);

                env.SendEventBean(new SupportBean("G1", 12));
                AssertViewData(
                    env,
                    12,
                    new[] {new object[] {"G1", 12}, new object[] {"G1", 11}},
                    10);

                env.SendEventBean(new SupportBean("G2", 22));
                AssertViewData(
                    env,
                    22,
                    new[] {new object[] {"G2", 22}, new object[] {"G2", 21}},
                    20);

                env.UndeployModuleContaining("s0");

                // test SODA
                env.UndeployAll();
                path.Clear();

                env.EplToModelCompileDeploy(contextEPL, path);

                // test built-in properties
                var fields = "c1,c2,c3,c4".SplitCsv();
                var ctx = "SegmentedByString";
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select context.name as c1, context.id as c2, context.key1 as c3, theString as c4 " +
                    "from SupportBean#length(2) as items",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {ctx, 0, "G1", "G1"});
                SupportContextPropUtil.AssertContextProps(
                    env,
                    "context",
                    "SegmentedByString",
                    new[] {0},
                    "key1",
                    new[] {new object[] {"G1"}});

                env.UndeployAll();

                // test grouped delivery
                path.Clear();
                env.CompileDeploy("@Name('var') create variable boolean trigger = false", path);
                env.CompileDeploy("create context MyCtx partition by theString from SupportBean", path);
                env.CompileDeploy(
                    "@Name('s0') context MyCtx select * from SupportBean#expr(not trigger) for grouped_delivery(TheString)",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "trigger", true);
                env.AdvanceTime(100);

                Assert.AreEqual(2, env.Listener("s0").NewDataList.Count);

                env.UndeployAll();
            }
        }

        public class ContextKeySegmentedViewSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@Name('CTX') create context SegmentedByString partition by theString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fields = "theString,intPrimitive".SplitCsv();
                var eplSelect = "@Name('S1') context SegmentedByString select irstream * from SupportBean#lastevent()";
                env.CompileDeploy(eplSelect, path).AddListener("S1");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 10});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"G1", 2},
                    new object[] {"G1", 1});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"G2", 11},
                    new object[] {"G2", 10});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoinWhereClauseOnPartitionKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyCtx partition by theString from SupportBean;\n" +
                          "@Name('select') context MyCtx select * from SupportBean#lastevent as sb, SupportBean_S0#lastevent as s0 " +
                          "where theString is 'Test'";
                env.CompileDeploy(epl).AddListener("select");

                env.SendEventBean(new SupportBean("Test", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.IsTrue(env.Listener("select").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedNullSingleKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context MyContext partition by theString from SupportBean", path);
                env.CompileDeploy("@Name('s0') context MyContext select count(*) as cnt from SupportBean", path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean(null, 10));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.SendEventBean(new SupportBean(null, 20));
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.SendEventBean(new SupportBean("A", 30));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedNullKeyMultiKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyContext partition by theString, intBoxed, intPrimitive from SupportBean",
                    path);
                env.CompileDeploy("@Name('s0') context MyContext select count(*) as cnt from SupportBean", path);
                env.AddListener("s0");

                SendSBEvent(env, "A", null, 1);
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                SendSBEvent(env, "A", null, 1);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                SendSBEvent(env, "A", 10, 1);
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context ByP0 as partition by theString from SupportBean terminated by SupportBean(intPrimitive<0)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context ByP0 select TheString, count(*) as cnt from SupportBean(intPrimitive>= 0)",
                    path);

                env.AddListener("s0");

                SendAssertSB(1, env, "A", 0);

                SendAssertSB(2, env, "A", 0);
                SendAssertNone(env, new SupportBean("A", -1));
                SendAssertSB(1, env, "A", 0);

                SendAssertSB(1, env, "B", 0);
                SendAssertNone(env, new SupportBean("B", -1));
                SendAssertSB(1, env, "B", 0);
                SendAssertSB(2, env, "B", 0);
                SendAssertNone(env, new SupportBean("B", -1));
                SendAssertSB(1, env, "B", 0);

                SendAssertNone(env, new SupportBean("C", -1));

                env.UndeployAll();
            }
        }

        internal class MySelectorFilteredPartitioned : ContextPartitionSelectorFiltered
        {
            private readonly ISet<int> cpids = new LinkedHashSet<int>();

            private readonly object[] match;

            internal MySelectorFilteredPartitioned(object[] match)
            {
                this.match = match;
            }

            public IList<object[]> Contexts { get; } = new List<object[]>();

            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierPartitioned) contextPartitionIdentifier;
                if (match == null && cpids.Contains(id.ContextPartitionId)) {
                    throw new EPException("Already exists context id: " + id.ContextPartitionId);
                }

                cpids.Add(id.ContextPartitionId);
                Contexts.Add(id.Keys);
                return Equals(id.Keys, match);
            }
        }
    }
} // end of namespace