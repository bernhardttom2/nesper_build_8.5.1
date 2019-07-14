///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using DescriptionAttribute = com.espertech.esper.common.client.annotation.DescriptionAttribute;
using SupportBeanComplexProps = com.espertech.esper.common.@internal.support.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherSelectExpr
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherPrecedenceNoColumnName());
            execs.Add(new EPLOtherGraphSelect());
            execs.Add(new EPLOtherKeywordsAllowed());
            execs.Add(new EPLOtherEscapeString());
            execs.Add(new EPLOtherGetEventType());
            execs.Add(new EPLOtherWindowStats());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            bool b,
            int i,
            float f1,
            float f2)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.BoolBoxed = b;
            bean.IntPrimitive = i;
            bean.FloatPrimitive = f1;
            bean.FloatBoxed = f2;
            env.SendEventBean(bean);
        }

        internal class EPLOtherPrecedenceNoColumnName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryPrecedenceNoColumnName(env, "3*2+1", "3*2+1", 7);
                TryPrecedenceNoColumnName(env, "(3*2)+1", "3*2+1", 7);
                TryPrecedenceNoColumnName(env, "3*(2+1)", "3*(2+1)", 9);
            }

            private static void TryPrecedenceNoColumnName(
                RegressionEnvironment env,
                string selectColumn,
                string expectedColumn,
                object value)
            {
                var epl = "@Name('s0') select " + selectColumn + " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                if (!env.Statement("s0").EventType.PropertyNames[0].Equals(expectedColumn)) {
                    Assert.Fail(
                        "Expected '" + expectedColumn + "' but was " + env.Statement("s0").EventType.PropertyNames[0]);
                }

                env.SendEventBean(new SupportBean("E1", 1));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(value, @event.Get(expectedColumn));
                env.UndeployAll();
            }
        }

        internal class EPLOtherGraphSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into MyStream select nested from SupportBeanComplexProps", path);
                var epl = "@Name('s0') select nested.nestedValue, nested.nestedNested.nestedNestedValue from MyStream";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                Assert.IsNotNull(env.Listener("s0").AssertOneGetNewAndReset());

                env.UndeployAll();
            }
        }

        internal class EPLOtherKeywordsAllowed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields =
                    "count,escape,every,sum,avg,max,min,coalesce,median,stddev,avedev,events,first,last,unidirectional,pattern,sql,metadatasql,prev,prior,weekday,lastweekday,cast,snapshot,variable,window,left,right,full,outer,join";
                env.CompileDeploy("@Name('s0') select " + fields + " from SupportBeanKeywords").AddListener("s0");

                env.SendEventBean(new SupportBeanKeywords());
                EPAssertionUtil.AssertEqualsExactOrder(env.Statement("s0").EventType.PropertyNames, fields.SplitCsv());

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();

                var fieldsArr = fields.SplitCsv();
                foreach (var aFieldsArr in fieldsArr) {
                    Assert.AreEqual(1, theEvent.Get(aFieldsArr));
                }

                env.UndeployAll();

                env.CompileDeploy(
                    "@Name('s0') select escape as stddev, count(*) as count, last from SupportBeanKeywords");
                env.AddListener("s0");
                env.SendEventBean(new SupportBeanKeywords());

                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(1, theEvent.Get("stddev"));
                Assert.AreEqual(1L, theEvent.Get("count"));
                Assert.AreEqual(1, theEvent.Get("last"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherEscapeString : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // The following EPL syntax compiles but fails to match a string "A'B", we are looking into:
                // env.CompileDeploy("@Name('s0') select * from SupportBean(string='A\\\'B')");

                TryEscapeMatch(env, "A'B", "\"A'B\""); // opposite quotes
                TryEscapeMatch(env, "A'B", "'A\\'B'"); // escape '
                TryEscapeMatch(env, "A'B", "'A\\u0027B'"); // unicode

                TryEscapeMatch(env, "A\"B", "'A\"B'"); // opposite quotes
                TryEscapeMatch(env, "A\"B", "'A\\\"B'"); // escape "
                TryEscapeMatch(env, "A\"B", "'A\\u0022B'"); // unicode

                env.CompileDeploy("@Name('A\\\'B') @Description(\"A\\\"B\") select * from SupportBean");
                Assert.AreEqual("A\'B", env.Statement("A\'B").Name);
                var desc = (DescriptionAttribute) env.Statement("A\'B").Annotations[1];
                Assert.AreEqual("A\"B", desc.Value);
                env.UndeployAll();

                env.CompileDeploy(
                    "@Name('s0') select 'volume' as field1, \"sleep\" as field2, \"\\u0041\" as unicodeA from SupportBean");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"field1", "field2", "unicodeA"},
                    new object[] {"volume", "sleep", "A"});
                env.UndeployAll();

                TryStatementMatch(env, "John's", "select * from SupportBean(theString='John\\'s')");
                TryStatementMatch(env, "John's", "select * from SupportBean(theString='John\\u0027s')");
                TryStatementMatch(
                    env,
                    "Quote \"Hello\"",
                    "select * from SupportBean(theString like \"Quote \\\"Hello\\\"\")");
                TryStatementMatch(
                    env,
                    "Quote \"Hello\"",
                    "select * from SupportBean(theString like \"Quote \\u0022Hello\\u0022\")");

                env.UndeployAll();
            }

            private static void TryEscapeMatch(
                RegressionEnvironment env,
                string property,
                string escaped)
            {
                var epl = "@Name('s0') select * from SupportBean(theString=" + escaped + ")";
                var text = "trying >" + escaped + "< (" + escaped.Length + " chars) EPL " + epl;
                log.Info("tryEscapeMatch for " + text);
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean(property, 1));
                Assert.AreEqual(env.Listener("s0").AssertOneGetNewAndReset().Get("IntPrimitive"), 1);
                env.UndeployAll();
            }

            private static void TryStatementMatch(
                RegressionEnvironment env,
                string property,
                string epl)
            {
                var text = "trying EPL " + epl;
                log.Info("tryEscapeMatch for " + text);
                env.CompileDeploy("@Name('s0') " + epl).AddListener("s0");
                env.SendEventBean(new SupportBean(property, 1));
                Assert.AreEqual(env.Listener("s0").AssertOneGetNewAndReset().Get("IntPrimitive"), 1);
                env.UndeployAll();
            }
        }

        internal class EPLOtherGetEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select TheString, boolBoxed aBool, 3*intPrimitive, floatBoxed+floatPrimitive result" +
                    " from SupportBean#length(3) " +
                    " where boolBoxed = true";
                env.CompileDeploy(epl).AddListener("s0");

                var type = env.Statement("s0").EventType;
                log.Debug(".testGetEventType properties=" + type.PropertyNames.RenderAny());
                EPAssertionUtil.AssertEqualsAnyOrder(
                    type.PropertyNames,
                    new[] {"3*intPrimitive", "TheString", "result", "aBool"});
                Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
                Assert.AreEqual(typeof(bool?), type.GetPropertyType("aBool"));
                Assert.AreEqual(typeof(float?), type.GetPropertyType("result"));
                Assert.AreEqual(typeof(int?), type.GetPropertyType("3*intPrimitive"));

                env.UndeployAll();
            }
        }

        internal class EPLOtherWindowStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select TheString, boolBoxed as aBool, 3*intPrimitive, floatBoxed+floatPrimitive as result" +
                    " from SupportBean#length(3) " +
                    " where boolBoxed = true";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "a", false, 0, 0, 0);
                SendEvent(env, "b", false, 0, 0, 0);
                Assert.IsTrue(env.Listener("s0").LastNewData == null);
                SendEvent(env, "c", true, 3, 10, 20);

                var received = env.Listener("s0").GetAndResetLastNewData()[0];
                Assert.AreEqual("c", received.Get("TheString"));
                Assert.AreEqual(true, received.Get("aBool"));
                Assert.AreEqual(30f, received.Get("result"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace