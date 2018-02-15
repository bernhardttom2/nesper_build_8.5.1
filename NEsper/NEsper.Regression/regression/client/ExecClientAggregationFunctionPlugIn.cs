///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientAggregationFunctionPlugIn : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddPlugInAggregationFunctionFactory("concatstring", typeof(MyConcatAggregationFunctionFactory).Name);
            configuration.AddPlugInAggregationFunctionFactory("concatstringTwo", typeof(MyConcatTwoAggFunctionFactory).Name);
            configuration.AddPlugInAggregationFunctionFactory("concat", typeof(SupportPluginAggregationMethodTwoFactory).Name);
            configuration.AddPlugInAggregationFunctionFactory("xxx", typeof(string).Name);
            configuration.AddPlugInAggregationFunctionFactory("yyy", "com.NoSuchClass");
            configuration.EngineDefaults.Threading.IsEngineFairlock = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionGrouped(epService);
            RunAssertionWindow(epService);
            RunAssertionDistinctAndStarParam(epService);
            RunAssertionArrayParamsAndDotMethod(epService);
            RunAssertionMultipleParams(epService);
            RunAssertionNoSubnodesRuntimeAdd(epService);
            RunAssertionMappedPropertyLookAlike(epService);
            RunAssertionFailedValidation(epService);
            RunAssertionInvalidUse(epService);
            RunAssertionInvalidConfigure(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionGrouped(EPServiceProvider epService) {
            string textOne = "select irstream CONCATSTRING(theString) as val from " + typeof(SupportBean).FullName + "#length(10) group by intPrimitive";
            TryGrouped(epService, textOne, null);
    
            string textTwo = "select irstream concatstring(theString) as val from " + typeof(SupportBean).FullName + "#win:Length(10) group by intPrimitive";
            TryGrouped(epService, textTwo, null);
    
            string textThree = "select irstream concatstring(theString) as val from " + typeof(SupportBean).FullName + "#length(10) group by intPrimitive";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(textThree);
            SerializableObjectCopier.Copy(model);
            Assert.AreEqual(textThree, model.ToEPL());
            TryGrouped(epService, null, model);
    
            string textFour = "select irstream concatstring(theString) as val from " + typeof(SupportBean).FullName + "#length(10) group by intPrimitive";
            var modelTwo = new EPStatementObjectModel();
            modelTwo.SelectClause = SelectClause.Create(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add(Expressions.PlugInAggregation("concatstring", Expressions.Property("theString")), "val");
            modelTwo.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView(null, "length", Expressions.Constant(10)));
            modelTwo.GroupByClause = GroupByClause.Create("intPrimitive");
            Assert.AreEqual(textFour, modelTwo.ToEPL());
            SerializableObjectCopier.Copy(modelTwo);
            TryGrouped(epService, null, modelTwo);
    
            string textFive = "select irstream ConcatstringTwo(theString) as val from " + typeof(SupportBean).FullName + "#length(10) group by intPrimitive";
            TryGrouped(epService, textFive, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryGrouped(EPServiceProvider epService, string text, EPStatementObjectModel model) {
            EPStatement statement;
            if (model != null) {
                statement = epService.EPAdministrator.Create(model);
            } else {
                statement = epService.EPAdministrator.CreateEPL(text);
            }
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a"}, new object[]{""});
    
            epService.EPRuntime.SendEvent(new SupportBean("b", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"b"}, new object[]{""});
    
            epService.EPRuntime.SendEvent(new SupportBean("c", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a c"}, new object[]{"a"});
    
            epService.EPRuntime.SendEvent(new SupportBean("d", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"b d"}, new object[]{"b"});
    
            epService.EPRuntime.SendEvent(new SupportBean("e", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a c e"}, new object[]{"a c"});
    
            epService.EPRuntime.SendEvent(new SupportBean("f", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"b d f"}, new object[]{"b d"});
    
            listener.Reset();
        }
    
        private void RunAssertionWindow(EPServiceProvider epService) {
            string text = "select irstream concatstring(theString) as val from " + typeof(SupportBean).FullName + "#length(2)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a"}, new object[]{""});
    
            epService.EPRuntime.SendEvent(new SupportBean("b", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a b"}, new object[]{"a"});
    
            epService.EPRuntime.SendEvent(new SupportBean("c", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"b c"}, new object[]{"a b"});
    
            epService.EPRuntime.SendEvent(new SupportBean("d", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"c d"}, new object[]{"b c"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDistinctAndStarParam(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // test *-parameter
            string textTwo = "select concatstring(*) as val from SupportBean";
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL(textTwo);
            var listenerTwo = new SupportUpdateListener();
            statementTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("d", -1));
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "val".Split(','), new object[]{"SupportBean(d, -1)"});
    
            epService.EPRuntime.SendEvent(new SupportBean("e", 2));
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), "val".Split(','), new object[]{"SupportBean(d, -1) SupportBean(e, 2)"});
    
            try {
                epService.EPAdministrator.CreateEPL("select concatstring(*) as val from SupportBean#lastevent, SupportBean unidirectional");
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'Concatstring(*)': The 'concatstring' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead");
            }
    
            // test distinct
            string text = "select irstream concatstring(distinct theString) as val from SupportBean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a"}, new object[]{""});
    
            epService.EPRuntime.SendEvent(new SupportBean("b", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a b"}, new object[]{"a"});
    
            epService.EPRuntime.SendEvent(new SupportBean("b", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a b"}, new object[]{"a b"});
    
            epService.EPRuntime.SendEvent(new SupportBean("c", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a b c"}, new object[]{"a b"});
    
            epService.EPRuntime.SendEvent(new SupportBean("a", -1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a b c"}, new object[]{"a b c"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionArrayParamsAndDotMethod(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("countback", typeof(SupportPluginAggregationMethodOneFactory).Name);
    
            string text = "select irstream Countback({1,2,intPrimitive}) as val from " + typeof(SupportBean).FullName;
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{-1}, new object[]{0});
    
            // test dot-method
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("myagg", typeof(MyAggFuncFactory).Name);
            string[] fields = "val0,val1".Split(',');
            epService.EPAdministrator.CreateEPL("select (Myagg(id)).TheString as val0, (Myagg(id)).IntPrimitive as val1 from SupportBean_A").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"XX", 1});
            Assert.AreEqual(1, MyAggFuncFactory.InstanceCount);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"XX", 2});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMultipleParams(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("countboundary", typeof(SupportPluginAggregationMethodThreeFactory).Name);
    
            TryAssertionMultipleParams(epService, false);
            TryAssertionMultipleParams(epService, true);
        }
    
        private void TryAssertionMultipleParams(EPServiceProvider epService, bool soda) {
    
            string text = "select irstream Countboundary(1,10,intPrimitive,*) as val from " + typeof(SupportBean).FullName;
            EPStatement statement = SupportModelHelper.CreateByCompileOrParse(epService, soda, text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            AggregationValidationContext validContext = SupportPluginAggregationMethodThreeFactory.Contexts[0];
            EPAssertionUtil.AssertEqualsExactOrder(new Type[]{typeof(int?), typeof(int?), typeof(int), typeof(SupportBean)}, validContext.ParameterTypes);
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{1, 10, null, null}, validContext.ConstantValues);
            EPAssertionUtil.AssertEqualsExactOrder(new bool[]{true, true, false, false}, validContext.IsConstantValue);
    
            var e1 = new SupportBean("E1", 5);
            epService.EPRuntime.SendEvent(e1);
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{1}, new object[]{0});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{1, 10, 5, e1}, SupportPluginAggregationMethodThree.LastEnterParameters);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{1}, new object[]{1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{1}, new object[]{1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{2}, new object[]{1});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNoSubnodesRuntimeAdd(EPServiceProvider epService) {
            string text = "select irstream Countback() as val from " + typeof(SupportBean).FullName;
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{-1}, new object[]{0});
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{-2}, new object[]{-1});
    
            statement.Dispose();
        }
    
        private void RunAssertionMappedPropertyLookAlike(EPServiceProvider epService) {
            string text = "select irstream concatstring('a') as val from " + typeof(SupportBean).FullName;
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("val"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a"}, new object[]{""});
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a a"}, new object[]{"a"});
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), "val", new object[]{"a a a"}, new object[]{"a a"});
    
            statement.Dispose();
        }
    
        private void RunAssertionFailedValidation(EPServiceProvider epService) {
            try {
                string text = "select Concat(1) from " + typeof(SupportBean).FullName;
                epService.EPAdministrator.CreateEPL(text);
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate select-clause expression 'Concat(1)': Plug-in aggregation function 'concat' failed validation: Invalid parameter type 'java.lang.int?', expecting string [");
            }
        }
    
        private void RunAssertionInvalidUse(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + " group by Xxx(1)",
                    "Error in expression: Error resolving aggregation: Aggregation class by name 'java.lang.string' does not implement AggregationFunctionFactory");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + " group by Yyy(1)",
                    "Error in expression: Error resolving aggregation: Could not load aggregation factory class by name 'com.NoSuchClass'");
        }
    
        private void RunAssertionInvalidConfigure(EPServiceProvider epService) {
            TryInvalidConfigure(epService, "a b", "MyClass");
            TryInvalidConfigure(epService, "abc", "My Type");
    
            // configure twice
            try {
                epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("concatstring", typeof(MyConcatAggregationFunction).Name);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        private void TryInvalidConfigure(EPServiceProvider epService, string funcName, string className) {
            try {
                epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(funcName, className);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                // expected
            }
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select Zzz(theString) from " + typeof(SupportBean).FullName,
                    "Error starting statement: Failed to validate select-clause expression 'Zzz(theString)': Unknown single-row function, aggregation function or mapped or indexed property named 'zzz' could not be resolved");
        }
    
        public class MyAggFuncFactory : AggregationFunctionFactory {
            private static int instanceCount;

            internal static int InstanceCount
            {
                get { return instanceCount; }
            }

            public string FunctionName
            {
                set { }
            }

            public void Validate(AggregationValidationContext validationContext) {
            }
    
            public AggregationMethod NewAggregator() {
                instanceCount++;
                return new MyAggFuncMethod();
            }

            public Type ValueType
            {
                get { return typeof(SupportBean); }
            }
        }
    
        public class MyAggFuncMethod : AggregationMethod {
    
            private int count;
    
            public void Enter(Object value) {
                count++;
            }
    
            public void Leave(Object value) {
                count--;
            }

            public object Value
            {
                get { return new SupportBean("XX", count); }
            }

            public void Clear() {
                count = 0;
            }
        }
    }
} // end of namespace
