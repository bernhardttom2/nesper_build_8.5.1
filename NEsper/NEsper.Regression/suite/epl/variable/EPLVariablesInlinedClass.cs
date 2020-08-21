///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
	public class EPLVariablesInlinedClass
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLVariablesInlinedClassLocal());
			execs.Add(new EPLVariablesInlinedClassGlobal());
			return execs;
		}

		private class EPLVariablesInlinedClassGlobal : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplClass = "@public @name('clazz') create inlined_class \"\"\"\n" +
				                  "[Serializable]\n" +
				                  "public class MyStateful {\n" +
				                  "    private string _value = \"X\";\n" +
				                  "    public string Value {\n" +
				                  "        get => _value;\n" +
				                  "        set => _value = value;\n" +
				                  "    }\n" +
				                  "}\n" +
				                  "\"\"\"\n";
				env.CompileDeploy(eplClass, path);

				string epl = "create variable MyStateful msf = new MyStateful();\n" +
				             "@Name('s0') select msf.value as c0 from SupportBean;\n" +
				             "on SupportBean_S0 set msf.setValue(p00);\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				SendAssert(env, "X");
				env.SendEventBean(new SupportBean_S0(1, "A"));
				SendAssert(env, "A");

				env.Milestone(0);

				SendAssert(env, "A");
				env.SendEventBean(new SupportBean_S0(2, "B"));
				SendAssert(env, "B");

				SupportDeploymentDependencies.AssertSingle(env, "s0", "clazz", EPObjectType.CLASSPROVIDED, "MyStateful");

				env.UndeployAll();
			}

			private void SendAssert(
				RegressionEnvironment env,
				string expected)
			{
				env.SendEventBean(new SupportBean());
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0".SplitCsv(), new object[] {expected});
			}
		}

		private class EPLVariablesInlinedClassLocal : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "inlined_class \"\"\"\n" +
				             "[Serializable]\n" +
				             "public class MyStateful {\n" +
				             "    private readonly int a;\n" +
				             "    private readonly int b;\n" +
				             "    public MyStateful(int a, int b) {\n" +
				             "        this.a = a;\n" +
				             "        this.b = b;\n" +
				             "    }\n" +
				             "    public int A {\n" +
				             "        get => a;\n" +
				             "        set => a = value;\n" +
				             "    }\n" +
				             "    public int B {\n" +
				             "        get => b;\n" +
				             "        set => b = value;\n" +
				             "    }\n" +
				             "}\n" +
				             "\"\"\"\n" +
				             "create variable MyStateful msf = new MyStateful(2, 3);\n" +
				             "@Name('s0') select msf.a as c0, msf.b as c1 from SupportBean;\n" +
				             "on SupportBeanNumeric set msf.setA(IntOne), msf.setB(intTwo);\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssert(env, 2, 3);

				env.Milestone(0);

				SendAssert(env, 2, 3);
				env.SendEventBean(new SupportBeanNumeric(10, 20));
				SendAssert(env, 10, 20);

				env.Milestone(1);

				SendAssert(env, 10, 20);

				env.UndeployAll();
			}

			private void SendAssert(
				RegressionEnvironment env,
				int expectedA,
				int expectedB)
			{
				env.SendEventBean(new SupportBean());
				AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1".SplitCsv(), new object[] {expectedA, expectedB});
			}
		}
	}
} // end of namespace
