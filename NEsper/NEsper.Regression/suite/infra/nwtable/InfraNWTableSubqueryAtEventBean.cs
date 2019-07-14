///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqueryAtEventBean
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraSubSelStar(true));
            execs.Add(new InfraSubSelStar(false));
            return execs;
        }

        private static void AssertReceived(
            SupportListener listener,
            object[][] values)
        {
            var @event = listener.AssertOneGetNewAndReset();
            var events = (EventBean[]) @event.GetFragment("detail");
            if (values == null) {
                Assert.IsNull(events);
                return;
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(events, "c0,c1".SplitCsv(), values);
        }

        internal class InfraSubSelStar : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSubSelStar(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfra#keepall as (c0 string, c1 int)"
                    : "create table MyInfra(c0 string primary key, c1 int)";
                env.CompileDeploy(eplCreate, path);

                // create insert into
                var eplInsert = "insert into MyInfra select TheString as c0, IntPrimitive as c1 from SupportBean";
                env.CompileDeploy(eplInsert, path);

                // create subquery
                var eplSubquery =
                    "@Name('s0') select p00, (select * from MyInfra) @eventbean as detail from SupportBean_S0";
                env.CompileDeploy(eplSubquery, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(0));
                AssertReceived(env.Listener("s0"), null);

                env.SendEventBean(new SupportBean("E1", 1));

                env.SendEventBean(new SupportBean_S0(0));
                AssertReceived(
                    env.Listener("s0"),
                    new[] {new object[] {"E1", 1}});

                env.SendEventBean(new SupportBean("E2", 2));

                env.SendEventBean(new SupportBean_S0(0));
                AssertReceived(
                    env.Listener("s0"),
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace