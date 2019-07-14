///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFIndex : IndexBackingTableInfo
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexChoiceJoin(true));
            execs.Add(new InfraSelectIndexChoiceJoin(false));
            execs.Add(new InfraSelectIndexChoice(true));
            execs.Add(new InfraSelectIndexChoice(false));
            return execs;
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            bool namedWindow,
            string[] indexes,
            object[] preloadedEvents,
            string datawindow,
            IndexAssertion[] assertions)
        {
            var path = new RegressionPath();
            var eplCreate = namedWindow
                ? "create window MyInfra." + datawindow + " as SupportSimpleBeanOne"
                : "create table MyInfra(s1 String primary key, i1 int primary key, d1 double primary key, l1 long primary key)";
            env.CompileDeploy(eplCreate, path);
            env.CompileDeploy("insert into MyInfra select s1,i1,d1,l1 from SupportSimpleBeanOne", path);
            foreach (var index in indexes) {
                env.CompileDeploy(index, path);
            }

            foreach (var @event in preloadedEvents) {
                env.SendEventBean(@event);
            }

            var count = 0;
            foreach (var assertion in assertions) {
                log.Info("======= Testing #" + count++);
                var epl = INDEX_CALLBACK_HOOK +
                          (assertion.Hint == null ? "" : assertion.Hint) +
                          "select * from MyInfra where " +
                          assertion.WhereClause;

                if (assertion.FafAssertion == null) {
                    try {
                        env.CompileExecuteFAF(epl, path);
                        Assert.Fail();
                    }
                    catch (Exception ex) {
                        // expected
                    }
                }
                else {
                    // assert index and access
                    var result = env.CompileExecuteFAF(epl, path);
                    SupportQueryPlanIndexHook.AssertFAFAndReset(
                        assertion.ExpectedIndexName,
                        assertion.IndexBackingClass);
                    assertion.FafAssertion.Invoke(result);
                }
            }

            env.UndeployAll();
        }

        internal class InfraSelectIndexChoiceJoin : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexChoiceJoin(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                object[] preloadedEventsOne = {
                    new SupportSimpleBeanOne("E1", 10, 1, 2),
                    new SupportSimpleBeanOne("E2", 11, 3, 4),
                    new SupportSimpleBeanTwo("E1", 20, 1, 2),
                    new SupportSimpleBeanTwo("E2", 21, 3, 4)
                };
                IndexAssertionFAF fafAssertion = result => {
                    var fields = "w1.s1,w2.s2,w1.i1,w2.i2".SplitCsv();
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        result.Array,
                        fields,
                        new[] {new object[] {"E1", "E1", 10, 20}, new object[] {"E2", "E2", 11, 21}});
                };

                IndexAssertion[] assertionsSingleProp = {
                    new IndexAssertion(null, "s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "s1 = s2 and l1 = l2", true, fafAssertion),
                    new IndexAssertion(null, "l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2", false, fafAssertion)
                };

                // single prop, no index, both declared unique (named window only)
                if (namedWindow) {
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        new string[0],
                        preloadedEventsOne,
                        "std:unique(s1)",
                        "std:unique(s2)",
                        assertionsSingleProp);
                }

                // single prop, unique indexes, both declared keepall
                string[] uniqueIndex = {"create unique index W1I1 on W1(s1)", "create unique index W1I2 on W2(s2)"};
                AssertIndexChoiceJoin(
                    env,
                    namedWindow,
                    uniqueIndex,
                    preloadedEventsOne,
                    "win:keepall()",
                    "win:keepall()",
                    assertionsSingleProp);

                // single prop, mixed indexes, both declared keepall
                IndexAssertion[] assertionsMultiProp = {
                    new IndexAssertion(null, "s1 = s2", false, fafAssertion),
                    new IndexAssertion(null, "s1 = s2 and l1 = l2", true, fafAssertion),
                    new IndexAssertion(null, "l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2 and s1 = s2", true, fafAssertion),
                    new IndexAssertion(null, "d1 = d2 and l1 = l2", false, fafAssertion)
                };
                if (namedWindow) {
                    string[] mixedIndex = {"create index W1I1 on W1(s1, l1)", "create unique index W1I2 on W2(s2)"};
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        mixedIndex,
                        preloadedEventsOne,
                        "std:unique(s1)",
                        "win:keepall()",
                        assertionsSingleProp);

                    // multi prop, no index, both declared unique
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        new string[0],
                        preloadedEventsOne,
                        "std:unique(s1, l1)",
                        "std:unique(s2, l2)",
                        assertionsMultiProp);
                }

                // multi prop, unique indexes, both declared keepall
                string[] uniqueIndexMulti =
                    {"create unique index W1I1 on W1(s1, l1)", "create unique index W1I2 on W2(s2, l2)"};
                AssertIndexChoiceJoin(
                    env,
                    namedWindow,
                    uniqueIndexMulti,
                    preloadedEventsOne,
                    "win:keepall()",
                    "win:keepall()",
                    assertionsMultiProp);

                // multi prop, mixed indexes, both declared keepall
                if (namedWindow) {
                    string[] mixedIndexMulti =
                        {"create index W1I1 on W1(s1)", "create unique index W1I2 on W2(s2, l2)"};
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        mixedIndexMulti,
                        preloadedEventsOne,
                        "std:unique(s1, l1)",
                        "win:keepall()",
                        assertionsMultiProp);
                }
            }

            private static void AssertIndexChoiceJoin(
                RegressionEnvironment env,
                bool namedWindow,
                string[] indexes,
                object[] preloadedEvents,
                string datawindowOne,
                string datawindowTwo,
                params IndexAssertion[] assertions)
            {
                var path = new RegressionPath();
                if (namedWindow) {
                    env.CompileDeploy("create window W1." + datawindowOne + " as SupportSimpleBeanOne", path);
                    env.CompileDeploy("create window W2." + datawindowTwo + " as SupportSimpleBeanTwo", path);
                }
                else {
                    env.CompileDeploy(
                        "create table W1 (s1 String primary key, i1 int primary key, d1 double primary key, l1 long primary key)",
                        path);
                    env.CompileDeploy(
                        "create table W2 (s2 String primary key, i2 int primary key, d2 double primary key, l2 long primary key)",
                        path);
                }

                env.CompileDeploy("insert into W1 select s1,i1,d1,l1 from SupportSimpleBeanOne", path);
                env.CompileDeploy("insert into W2 select s2,i2,d2,l2 from SupportSimpleBeanTwo", path);

                foreach (var index in indexes) {
                    env.CompileDeploy(index, path);
                }

                foreach (var @event in preloadedEvents) {
                    env.SendEventBean(@event);
                }

                var count = 0;
                foreach (var assertion in assertions) {
                    log.Info("======= Testing #" + count++);
                    var epl = INDEX_CALLBACK_HOOK +
                              (assertion.Hint == null ? "" : assertion.Hint) +
                              "select * from W1 as w1, W2 as w2 " +
                              "where " +
                              assertion.WhereClause;
                    EPFireAndForgetQueryResult result = null;
                    try {
                        result = env.CompileExecuteFAF(epl, path);
                    }
                    catch (EPCompileExceptionItem ex) {
                        log.Error("Failed to process:" + ex.Message, ex);
                        if (assertion.EventSendAssertion == null) {
                            // no assertion, expected
                            Assert.IsTrue(ex.Message.Contains("index hint busted"));
                            continue;
                        }

                        throw new EPException("Unexpected statement exception: " + ex.Message, ex);
                    }

                    // assert index and access
                    SupportQueryPlanIndexHook.AssertJoinAllStreamsAndReset(assertion.Unique);
                    assertion.FafAssertion.Invoke(result);
                }

                env.UndeployAll();
            }
        }

        internal class InfraSelectIndexChoice : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexChoice(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                object[] preloadedEventsOne =
                    {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
                IndexAssertionFAF fafAssertion = result => {
                    var fields = "s1,i1".SplitCsv();
                    EPAssertionUtil.AssertPropsPerRow(
                        result.Array,
                        fields,
                        new[] {new object[] {"E2", 20}});
                };

                // single index one field (plus declared unique)
                var noindexes = new string[0];
                AssertIndexChoice(
                    env,
                    namedWindow,
                    noindexes,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "s1 = 'E2'", null, null, fafAssertion),
                        new IndexAssertion(null, "s1 = 'E2' and l1 = 22", null, null, fafAssertion),
                        new IndexAssertion("@Hint('index(One)')", "s1 = 'E2' and l1 = 22", null, null, fafAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "s1 = 'E2' and l1 = 22") // should bust
                    });

                // single index one field (plus declared unique)
                string[] indexOneField = {"create unique index One on MyInfra (s1)"};
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexOneField,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "s1 = 'E2'", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                        new IndexAssertion(null, "s1 in ('E2')", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                        new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_UNIQUE, fafAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "s1 = 'E2' and l1 = 22",
                            "One",
                            BACKING_SINGLE_UNIQUE,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "s1 = 'E2' and l1 = 22") // should bust
                    });

                // single index two field (plus declared unique)
                string[] indexTwoField = {"create unique index One on MyInfra (s1, l1)"};
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexTwoField,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "s1 = 'E2'", null, null, fafAssertion),
                        new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", BACKING_MULTI_UNIQUE, fafAssertion)
                    });

                // two index one unique (plus declared unique)
                string[] indexSetTwo = {
                    "create index One on MyInfra (s1)",
                    "create unique index Two on MyInfra (s1, d1)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetTwo,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "s1 = 'E2'", "One", BACKING_SINGLE_DUPS, fafAssertion),
                        new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", BACKING_SINGLE_DUPS, fafAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "s1 = 'E2' and l1 = 22",
                            "One",
                            BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion(
                            "@Hint('index(Two,One)')",
                            "s1 = 'E2' and l1 = 22",
                            "One",
                            BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "s1 = 'E2' and l1 = 22"), // busted
                        new IndexAssertion(
                            "@Hint('index(explicit,bust)')",
                            "s1 = 'E2' and l1 = 22",
                            "One",
                            BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = 'E2' and d1 = 21 and l1 = 22",
                            "Two",
                            BACKING_MULTI_UNIQUE,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = 22 and l1 = 22") // busted
                    });

                // range (unique)
                string[] indexSetThree = {
                    "create index One on MyInfra (l1 btree)",
                    "create index Two on MyInfra (d1 btree)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetThree,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "l1 between 22 and 23", "One", BACKING_SORTED, fafAssertion),
                        new IndexAssertion(null, "d1 between 21 and 22", "Two", BACKING_SORTED, fafAssertion),
                        new IndexAssertion("@Hint('index(One, bust)')", "d1 between 21 and 22") // busted
                    });
            }
        }
    }
} // end of namespace