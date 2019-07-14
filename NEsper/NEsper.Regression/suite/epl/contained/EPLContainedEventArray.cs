///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
    public class EPLContainedEventArray
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLContainedEventDocSample());
            execs.Add(new EPLContainedEventIntArray());
            return execs;
        }

        private static void AssertCount(
            RegressionEnvironment env,
            RegressionPath path,
            long i)
        {
            Assert.AreEqual(i, env.CompileExecuteFAF("select count(*) as c0 from MyWindow", path).Array[0].Get("c0"));
        }

        internal class EPLContainedEventDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create schema IdContainer(id int);" +
                    "create schema MyEvent(ids int[]);" +
                    "select * from MyEvent[ids@type(IdContainer)];",
                    path);

                env.CompileDeploy(
                    "create window MyWindow#keepall (id int);" +
                    "on MyEvent[ids@type(IdContainer)] as my_ids \n" +
                    "delete from MyWindow my_window \n" +
                    "where my_ids.id = my_window.id;",
                    path);

                env.UndeployAll();
            }
        }

        internal class EPLContainedEventIntArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create objectarray schema DeleteId(id int);" +
                          "create window MyWindow#keepall as SupportBean;" +
                          "insert into MyWindow select * from SupportBean;" +
                          "on SupportBeanArrayCollMap[intArr@type(DeleteId)] delete from MyWindow where IntPrimitive = id";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                AssertCount(env, path, 2);
                env.SendEventBean(new SupportBeanArrayCollMap(new[] {1, 2}));
                AssertCount(env, path, 0);

                env.UndeployAll();
            }
        }
    }
} // end of namespace