///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileEnginePath
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEnginePathObjectTypes(execs);
            WithEnginePathInfraWithIndex(execs);
            WithEnginePathPreconfiguredEventTypeFromPath(execs);
            WithrEnginePathNamedWindowUse(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithrEnginePathNamedWindowUse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompilerEnginePathNamedWindowUse());
            return execs;
        }

        public static IList<RegressionExecution> WithEnginePathPreconfiguredEventTypeFromPath(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileEnginePathPreconfiguredEventTypeFromPath());
            return execs;
        }

        public static IList<RegressionExecution> WithEnginePathInfraWithIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileEnginePathInfraWithIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithEnginePathObjectTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileEnginePathObjectTypes());
            return execs;
        }

        private class ClientCompilerEnginePathNamedWindowUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                CreateStmt(env, "@public @buseventtype create schema Event(string_field string, double_field double)");
                CreateStmt(env, "@public create window EventWindow#time(600L) as select * from Event");
                CreateStmt(env, "insert into EventWindow select * from Event");
                CreateStmt(env, "select sum(double_field) AS sum_double_field, string_field, window() from EventWindow");

                env.UndeployAll();
            }
        }

        public class ClientCompileEnginePathPreconfiguredEventTypeFromPath : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                CreateStmt(
                    env,
                    "@Name('A') @public create table MyTableAggs(TheString string primary key, thecnt count(*), thewin window(*) @type(SupportBean))");
                CreateStmt(
                    env,
                    "@Name('B') into table MyTableAggs select count(*) as thecnt, window(*) as thewin from SupportBean#keepall() group by TheString");

                env.UndeployAll();
            }
        }

        public class ClientCompileEnginePathInfraWithIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                //CreateStmt(env, "@Name('Create') @public create table MyTable(id string primary key, theGroup int primary key)");
                //CreateStmt(env, "@Name('Index') create unique index I1 on MyTable(id)");

                CreateStmt(env, "@Name('Create') @public create window MyWindow#keepall as SupportBean");
                CreateStmt(env, "@Name('Index') create unique index I1 on MyWindow(TheString)");

                env.UndeployAll();
            }
        }

        public class ClientCompileEnginePathObjectTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var deployed = "create variable int myvariable = 10;\n" +
                               "create schema MySchema();\n" +
                               "create expression myExpr { 'abc' };\n" +
                               "create window MyWindow#keepall as SupportBean_S0;\n" +
                               "create table MyTable(y string);\n" +
                               "create context MyContext start SupportBean_S0 end SupportBean_S1;\n" +
                               "create expression myScript() [ return 2; ];\n" +
                               "create inlined_class \"\"\" public class MyClass { public static string DoIt(string parameter) { return \"def\"; } }\"\"\";\n";
                env.CompileDeploy(deployed, new RegressionPath());

                var epl = "@Name('s0') select myvariable as c0, myExpr() as c1, myScript() as c2, preconfigured_variable as c3," +
                          "MyClass.DoIt(TheString) as c4 from SupportBean;\n" +
                          "select * from MySchema;" +
                          "on SupportBean_S1 delete from MyWindow;\n" +
                          "on SupportBean_S1 delete from MyTable;\n" +
                          "context MyContext select * from SupportBean;\n";
                CompileDeployWEnginePath(env, epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1,c2,c3,c4".SplitCsv(),
                    new object[] {10, "abc", 2, 5, "def"});

                env.UndeployAll();
            }
        }

        private static RegressionEnvironment CompileDeployWEnginePath(
            RegressionEnvironment env,
            string epl)
        {
            EPCompiled compiled;
            try {
                compiled = CompileWEnginePathAndEmptyConfig(env, epl);
            }
            catch (EPCompileException ex) {
                throw new EPRuntimeException(ex);
            }

            env.Deploy(compiled);
            return env;
        }

        private static EPCompiled CompileWEnginePathAndEmptyConfig(
            RegressionEnvironment env,
            string epl)
        {
            var args = new CompilerArguments(env.Configuration);
            args.Path.Add(env.Runtime.RuntimePath);
            return env.Compiler.Compile(epl, args);
        }

        private static EPDeployment CreateStmt(
            RegressionEnvironment env,
            string epl)
        {
            try {
                var configuration = env.Runtime.ConfigurationDeepCopy;
                var args = new CompilerArguments(configuration);
                args.Path.Add(env.Runtime.RuntimePath);
                var compiled = env.Compiler.Compile(epl, args);
                return env.Runtime.DeploymentService.Deploy(compiled);
            }
            catch (Exception ex) {
                throw new EPRuntimeException(ex);
            }
        }
    }
} // end of namespace