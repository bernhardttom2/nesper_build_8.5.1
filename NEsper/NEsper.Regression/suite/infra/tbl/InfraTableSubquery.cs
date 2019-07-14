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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableSubquery
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraTableSubqueryAgainstKeyed());
            execs.Add(new InfraTableSubqueryAgainstUnkeyed());
            execs.Add(new InfraTableSubquerySecondaryIndex());
            return execs;
        }

        private static void AssertValues(
            RegressionEnvironment env,
            string keys,
            int?[] values)
        {
            var keyarr = keys.SplitCsv();
            for (var i = 0; i < keyarr.Length; i++) {
                env.SendEventBean(new SupportBean_S0(0, keyarr[i]));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(
                    values[i],
                    @event.Get("value"),
                    "Failed for key '" + keyarr[i] + "'");
            }
        }

        private static void SendInsertUpdate(
            RegressionEnvironment env,
            string p00,
            string p01,
            string p02,
            int value)
        {
            env.SendEventBean(new SupportBean_S0(value, p00, p01, p02));
        }

        private static void AssertSubselect(
            RegressionEnvironment env,
            string @string,
            int? expectedSum)
        {
            var fields = "c0".SplitCsv();
            env.SendEventBean(new SupportBean(@string, -1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {expectedSum});
        }

        internal class InfraTableSubqueryAgainstKeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                env.CompileDeploy(
                    "create table varagg as (" +
                    "key string primary key, total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table varagg " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString",
                    path);
                env.CompileDeploy(
                        "@Name('s0') select (select total from varagg where key = s0.p00) as value " +
                        "from SupportBean_S0 as s0",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("G2", 200));
                AssertValues(env, "G1,G2", new int?[] {null, 200});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 100));
                AssertValues(env, "G1,G2", new int?[] {100, 200});

                env.UndeployAll();
            }
        }

        internal class InfraTableSubqueryAgainstUnkeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                env.CompileDeploy("create table InfraOne (string string, IntPrimitive int)", path);
                env.CompileDeploy(
                        "@Name('s0') select (select IntPrimitive from InfraOne where string = s0.p00) as c0 from SupportBean_S0 as s0",
                        path)
                    .AddListener("s0");
                env.CompileDeploy(
                    "insert into InfraOne select TheString as string, IntPrimitive from SupportBean",
                    path);

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0".SplitCsv(),
                    new object[] {10});

                env.UndeployAll();
            }
        }

        internal class InfraTableSubquerySecondaryIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var eplTable =
                    "create table MyTable(k0 string primary key, k1 string primary key, p2 string, value int)";
                env.CompileDeploy(eplTable, path);

                var eplIndex = "create index MyIndex on MyTable(p2)";
                env.CompileDeploy(eplIndex, path);

                var eplInto = "on SupportBean_S0 merge MyTable " +
                              "where p00 = k0 and p01 = k1 " +
                              "when not matched then insert select p00 as k0, p01 as k1, p02 as p2, id as value " +
                              "when matched then update set p2 = p02, value = id ";
                env.CompileDeploy(eplInto, path);

                var eplSubselect =
                    "@Name('s0') select (select value from MyTable as tbl where sb.TheString = tbl.p2) as c0 from SupportBean as sb";
                env.CompileDeploy(eplSubselect, path).AddListener("s0");

                SendInsertUpdate(env, "G1", "SG1", "P2_1", 10);
                AssertSubselect(env, "P2_1", 10);

                env.Milestone(0);

                SendInsertUpdate(env, "G1", "SG1", "P2_2", 11);

                env.Milestone(1);

                AssertSubselect(env, "P2_1", null);
                AssertSubselect(env, "P2_2", 11);

                env.UndeployAll();
            }
        }
    }
} // end of namespace