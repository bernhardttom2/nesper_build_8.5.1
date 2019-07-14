///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyNestedSimple : RegressionExecution
    {
        public delegate void FunctionSendEvent4Int(
            RegressionEnvironment env,
            int lvl1,
            int lvl2,
            int lvl3,
            int lvl4);

        public static readonly string XML_TYPENAME = typeof(EventInfraPropertyNestedSimple).FullName + "XML";
        public static readonly string MAP_TYPENAME = typeof(EventInfraPropertyNestedSimple).FullName + "Map";
        public static readonly string OA_TYPENAME = typeof(EventInfraPropertyNestedSimple).FullName + "OA";
        public static readonly string AVRO_TYPENAME = typeof(EventInfraPropertyNestedSimple).FullName + "Avro";
        private static readonly string BEAN_TYPENAME = typeof(InfraNestedSimplePropTop).Name;

        private static readonly FunctionSendEvent4Int FMAP = (
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = Collections.SingletonDataMap("lvl4", lvl4);
            var l3 = TwoEntryMap<string, object>("l4", l4, "lvl3", lvl3);
            var l2 = TwoEntryMap<string, object>("l3", l3, "lvl2", lvl2);
            var l1 = TwoEntryMap<string, object>("l2", l2, "lvl1", lvl1);
            var top = Collections.SingletonDataMap("l1", l1);
            env.SendEventMap(top, MAP_TYPENAME);
        };

        private static readonly FunctionSendEvent4Int FOA = (
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            object[] l4 = {lvl4};
            object[] l3 = {l4, lvl3};
            object[] l2 = {l3, lvl2};
            object[] l1 = {l2, lvl1};
            object[] top = {l1};
            env.SendEventObjectArray(top, OA_TYPENAME);
        };

        private static readonly FunctionSendEvent4Int FBEAN = (
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = new InfraNestedSimplePropLvl4(lvl4);
            var l3 = new InfraNestedSimplePropLvl3(l4, lvl3);
            var l2 = new InfraNestedSimplePropLvl2(l3, lvl2);
            var l1 = new InfraNestedSimplePropLvl1(l2, lvl1);
            var top = new InfraNestedSimplePropTop(l1);
            env.SendEventBean(top);
        };

        private static readonly FunctionSendEvent4Int FXML = (
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                      "<myevent>\n" +
                      "\t<l1 lvl1=\"${lvl1}\">\n" +
                      "\t\t<l2 lvl2=\"${lvl2}\">\n" +
                      "\t\t\t<l3 lvl3=\"${lvl3}\">\n" +
                      "\t\t\t\t<l4 lvl4=\"${lvl4}\">\n" +
                      "\t\t\t\t</l4>\n" +
                      "\t\t\t</l3>\n" +
                      "\t\t</l2>\n" +
                      "\t</l1>\n" +
                      "</myevent>";
            xml = xml.Replace("${lvl1}", Convert.ToString(lvl1));
            xml = xml.Replace("${lvl2}", Convert.ToString(lvl2));
            xml = xml.Replace("${lvl3}", Convert.ToString(lvl3));
            xml = xml.Replace("${lvl4}", Convert.ToString(lvl4));
            try {
                SupportXML.SendXMLEvent(env, xml, XML_TYPENAME);
            }
            catch (Exception e) {
                throw new EPException(e);
            }
        };

        private static readonly FunctionSendEvent4Int FAVRO = (
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var schema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            var lvl1Schema = schema.GetField("l1").Schema.AsRecordSchema();
            var lvl2Schema = lvl1Schema.GetField("l2").Schema.AsRecordSchema();
            var lvl3Schema = lvl2Schema.GetField("l3").Schema.AsRecordSchema();
            var lvl4Schema = lvl3Schema.GetField("l4").Schema.AsRecordSchema();
            var lvl4Rec = new GenericRecord(lvl4Schema);
            lvl4Rec.Put("lvl4", lvl4);
            var lvl3Rec = new GenericRecord(lvl3Schema);
            lvl3Rec.Put("l4", lvl4Rec);
            lvl3Rec.Put("lvl3", lvl3);
            var lvl2Rec = new GenericRecord(lvl2Schema);
            lvl2Rec.Put("l3", lvl3Rec);
            lvl2Rec.Put("lvl2", lvl2);
            var lvl1Rec = new GenericRecord(lvl1Schema);
            lvl1Rec.Put("l2", lvl2Rec);
            lvl1Rec.Put("lvl1", lvl1);
            var datum = new GenericRecord(schema);
            datum.Put("l1", lvl1Rec);
            env.SendEventAvro(datum, AVRO_TYPENAME);
        };

        public void Run(RegressionEnvironment env)
        {
            RunAssertion(
                env,
                BEAN_TYPENAME,
                FBEAN,
                typeof(InfraNestedSimplePropLvl1),
                typeof(InfraNestedSimplePropLvl1).FullName);
            RunAssertion(env, MAP_TYPENAME, FMAP, typeof(IDictionary<string, object>), MAP_TYPENAME + "_1");
            RunAssertion(env, OA_TYPENAME, FOA, typeof(object[]), OA_TYPENAME + "_1");
            RunAssertion(env, XML_TYPENAME, FXML, typeof(XmlNode), XML_TYPENAME + ".l1");
            RunAssertion(env, AVRO_TYPENAME, FAVRO, typeof(GenericRecord), AVRO_TYPENAME + "_1");
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4Int send,
            Type nestedClass,
            string fragmentTypeName)
        {
            RunAssertionSelectNested(env, typename, send);
            RunAssertionBeanNav(env, typename, send);
            RunAssertionTypeValidProp(env, typename, send, nestedClass, fragmentTypeName);
            RunAssertionTypeInvalidProp(env, typename);
        }

        private void RunAssertionBeanNav(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4Int send)
        {
            var epl = "@Name('s0') select * from " + typename;
            env.CompileDeploy(epl).AddListener("s0");

            send.Invoke(env, 1, 2, 3, 4);
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                "l1.lvl1,l1.l2.lvl2,l1.l2.l3.lvl3,l1.l2.l3.l4.lvl4".SplitCsv(),
                new object[] {1, 2, 3, 4});
            SupportEventTypeAssertionUtil.AssertConsistency(@event);
            SupportEventTypeAssertionUtil.AssertFragments(@event, typename.Equals(BEAN_TYPENAME), false, "l1.l2");
            SupportEventTypeAssertionUtil.AssertFragments(
                @event,
                typename.Equals(BEAN_TYPENAME),
                false,
                "l1,l1.l2,l1.l2.l3,l1.l2.l3.l4");
            RunAssertionEventInvalidProp(@event);

            env.UndeployAll();
        }

        private void RunAssertionSelectNested(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4Int send)
        {
            var epl = "@Name('s0') select " +
                      "l1.lvl1 as c0, " +
                      "exists(l1.lvl1) as exists_c0, " +
                      "l1.l2.lvl2 as c1, " +
                      "exists(l1.l2.lvl2) as exists_c1, " +
                      "l1.l2.l3.lvl3 as c2, " +
                      "exists(l1.l2.l3.lvl3) as exists_c2, " +
                      "l1.l2.l3.l4.lvl4 as c3, " +
                      "exists(l1.l2.l3.l4.lvl4) as exists_c3 " +
                      "from " +
                      typename;
            env.CompileDeploy(epl).AddListener("s0");
            var fields = "c0,exists_c0,c1,exists_c1,c2,exists_c2,c3,exists_c3".SplitCsv();

            var eventType = env.Statement("s0").EventType;
            foreach (var property in fields) {
                Assert.AreEqual(
                    property.StartsWith("exists") ? typeof(bool?) : typeof(int?),
                    eventType.GetPropertyType(property).GetBoxedType());
            }

            send.Invoke(env, 1, 2, 3, 4);
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {1, true, 2, true, 3, true, 4, true});
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            send.Invoke(env, 10, 5, 50, 400);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, true, 5, true, 50, true, 400, true});

            env.UndeployAll();
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Arrays.AsList("l2", "l1.l3", "l1.xxx", "l1.l2.x", "l1.l2.l3.x", "l1.lvl1.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            string typeName,
            FunctionSendEvent4Int send,
            Type nestedClass,
            string fragmentTypeName)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            object[][] expectedType = {
                new object[] {"l1", nestedClass, fragmentTypeName, false}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new[] {"l1"}, eventType.PropertyNames);

            foreach (var prop in Arrays.AsList("l1", "l1.lvl1", "l1.l2", "l1.l2.lvl2")) {
                Assert.IsNotNull(eventType.GetGetter(prop));
                Assert.IsTrue(eventType.IsProperty(prop));
            }

            Assert.AreEqual(nestedClass, eventType.GetPropertyType("l1"));
            foreach (var prop in Arrays.AsList("l1.lvl1", "l1.l2.lvl2", "l1.l2.l3.lvl3")) {
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType(prop).GetBoxedType());
            }

            var lvl1Fragment = eventType.GetFragmentType("l1");
            Assert.IsFalse(lvl1Fragment.IsIndexed);
            Assert.AreEqual(send == FBEAN, lvl1Fragment.IsNative);
            Assert.AreEqual(fragmentTypeName, lvl1Fragment.FragmentType.Name);

            var lvl2Fragment = eventType.GetFragmentType("l1.l2");
            Assert.IsFalse(lvl2Fragment.IsIndexed);
            Assert.AreEqual(send == FBEAN, lvl2Fragment.IsNative);

            Assert.AreEqual(
                new EventPropertyDescriptor("l1", nestedClass, null, false, false, false, false, true),
                eventType.GetPropertyDescriptor("l1"));
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            foreach (var prop in Arrays.AsList(
                "l2",
                "l1.l3",
                "l1.lvl1.lvl1",
                "l1.l2.l4",
                "l1.l2.xx",
                "l1.l2.l3.lvl5")) {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Assert.AreEqual(null, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
            }
        }

        [Serializable]
        public class InfraNestedSimplePropTop
        {
            public InfraNestedSimplePropTop(InfraNestedSimplePropLvl1 l1)
            {
                L1 = l1;
            }

            public InfraNestedSimplePropLvl1 L1 { get; }
        }

        public class InfraNestedSimplePropLvl1
        {
            public InfraNestedSimplePropLvl1(
                InfraNestedSimplePropLvl2 l2,
                int lvl1)
            {
                L2 = l2;
                Lvl1 = lvl1;
            }

            public InfraNestedSimplePropLvl2 L2 { get; }

            public int Lvl1 { get; }
        }

        public class InfraNestedSimplePropLvl2
        {
            public InfraNestedSimplePropLvl2(
                InfraNestedSimplePropLvl3 l3,
                int lvl2)
            {
                L3 = l3;
                Lvl2 = lvl2;
            }

            public InfraNestedSimplePropLvl3 L3 { get; }

            public int Lvl2 { get; }
        }

        public class InfraNestedSimplePropLvl3
        {
            public InfraNestedSimplePropLvl3(
                InfraNestedSimplePropLvl4 l4,
                int lvl3)
            {
                L4 = l4;
                Lvl3 = lvl3;
            }

            public InfraNestedSimplePropLvl4 L4 { get; }

            public int Lvl3 { get; }
        }

        public class InfraNestedSimplePropLvl4
        {
            public InfraNestedSimplePropLvl4(int lvl4)
            {
                Lvl4 = lvl4;
            }

            public int Lvl4 { get; }
        }
    }
} // end of namespace