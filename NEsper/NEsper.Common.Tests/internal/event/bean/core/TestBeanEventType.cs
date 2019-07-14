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
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    [TestFixture]
    public class TestBeanEventType : AbstractTestBase
    {
        [SetUp]
        public void SetUp()
        {
            eventTypeSimple = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
            eventTypeComplex = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanComplexProps));
            eventTypeNested = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanCombinedProps));

            objSimple = new SupportBeanSimple("a", 20);
            objComplex = SupportBeanComplexProps.MakeDefaultBean();
            objCombined = SupportBeanCombinedProps.MakeDefaultBean();

            eventSimple = new BeanEventBean(objSimple, eventTypeSimple);
            eventComplex = new BeanEventBean(objComplex, eventTypeComplex);
            eventNested = new BeanEventBean(objCombined, eventTypeNested);
        }

        private BeanEventType eventTypeSimple;
        private BeanEventType eventTypeComplex;
        private BeanEventType eventTypeNested;

        private EventBean eventSimple;
        private EventBean eventComplex;
        private EventBean eventNested;

        private SupportBeanSimple objSimple;
        private SupportBeanComplexProps objComplex;
        private SupportBeanCombinedProps objCombined;

        private static void TryInvalidIsProperty(
            BeanEventType type,
            string property)
        {
            try
            {
                type.GetPropertyType(property);
                Assert.Fail();
            }
            catch (PropertyAccessException)
            {
                // expected
            }
        }

        private static void RunTest(
            IList<PropTestDesc> tests,
            BeanEventType eventType,
            EventBean eventBean)
        {
            foreach (var desc in tests)
            {
                RunTest(desc, eventType, eventBean);
            }
        }

        private static void RunTest(
            PropTestDesc test,
            BeanEventType eventType,
            EventBean eventBean)
        {
            var propertyName = test.PropertyName;

            Assert.AreEqual(
                test.IsProperty,
                eventType.IsProperty(propertyName),
                "isProperty mismatch on '" + propertyName + "',");
            Assert.AreEqual(
                test.Clazz,
                eventType.GetPropertyType(propertyName),
                "getPropertyType mismatch on '" + propertyName + "',");

            var getter = eventType.GetGetter(propertyName);
            if (getter == null)
            {
                Assert.IsFalse(test.IsHasGetter, "getGetter null on '" + propertyName + "',");
            }
            else
            {
                Assert.IsTrue(test.IsHasGetter, "getGetter not null on '" + propertyName + "',");
                if (ReferenceEquals(test.GetterReturnValue, typeof(NullReferenceException)))
                {
                    try
                    {
                        getter.Get(eventBean);
                        Assert.Fail("getGetter not throwing null pointer on '" + propertyName);
                    }
                    catch (NullReferenceException)
                    {
                        // expected
                    }
                }
                else
                {
                    var value = getter.Get(eventBean);
                    Assert.AreEqual(
                        test.GetterReturnValue,
                        value,
                        "getter value mismatch on '" + propertyName + "',");
                }
            }
        }

        public class PropTestDesc
        {
            public PropTestDesc(
                string propertyName,
                bool property,
                Type clazz,
                bool hasGetter,
                object getterReturnValue)
            {
                PropertyName = propertyName;
                IsProperty = property;
                Clazz = clazz;
                IsHasGetter = hasGetter;
                GetterReturnValue = getterReturnValue;
            }

            public string PropertyName { get; }

            public bool IsProperty { get; }

            public Type Clazz { get; }

            public bool IsHasGetter { get; }

            public object GetterReturnValue { get; }
        }

        [Test]
        public void TestFragments()
        {
            var nestedTypeFragment = eventTypeComplex.GetFragmentType("Nested");
            var nestedType = nestedTypeFragment.FragmentType;
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested).Name, nestedType.Name);
            Assert.AreEqual(typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), nestedType.UnderlyingType);
            Assert.AreEqual(typeof(string), nestedType.GetPropertyType("NestedValue"));
            Assert.IsNull(eventTypeComplex.GetFragmentType("Indexed[0]"));

            nestedTypeFragment = eventTypeNested.GetFragmentType("Indexed[0]");
            nestedType = nestedTypeFragment.FragmentType;
            Assert.IsFalse(nestedTypeFragment.IsIndexed);
            Assert.AreEqual(typeof(SupportBeanCombinedProps.NestedLevOne).Name, nestedType.Name);
            Assert.AreEqual(typeof(IDictionary<string, object>), nestedType.GetPropertyType("mapprop"));

            SupportEventTypeAssertionUtil.AssertConsistency(eventTypeComplex);
            SupportEventTypeAssertionUtil.AssertConsistency(eventTypeNested);
        }

        [Test]
        public void TestGetGetter()
        {
            Assert.AreEqual(null, eventTypeSimple.GetGetter("dummy"));

            var getter = eventTypeSimple.GetGetter("myInt");
            Assert.AreEqual(20, getter.Get(eventSimple));
            getter = eventTypeSimple.GetGetter("myString");
            Assert.AreEqual("a", getter.Get(eventSimple));
        }

        [Test]
        public void TestGetPropertyNames()
        {
            var properties = eventTypeSimple.PropertyNames;
            Assert.IsTrue(properties.Length == 2);
            Assert.IsTrue(properties[0].Equals("myInt"));
            Assert.IsTrue(properties[1].Equals("myString"));
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("myInt", typeof(int), null, false, false, false, false, false),
                    new EventPropertyDescriptor("myString", typeof(string), null, false, false, false, false, false)
                },
                eventTypeSimple.PropertyDescriptors);

            properties = eventTypeComplex.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(SupportBeanComplexProps.PROPERTIES, properties);

            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("SimpleProperty", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("mapProperty", typeof(IDictionary<string, object>), typeof(string), false, false, false, true, false),
                    new EventPropertyDescriptor("Mapped", typeof(string), typeof(object), false, true, false, true, false),
                    new EventPropertyDescriptor("Indexed", typeof(int), null, true, false, true, false, false),
                    new EventPropertyDescriptor(
                        "Nested",
                        typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested),
                        null,
                        false,
                        false,
                        false,
                        false,
                        true),
                    new EventPropertyDescriptor("arrayProperty", typeof(int[]), typeof(int), false, false, true, false, false),
                    new EventPropertyDescriptor("objectArray", typeof(object[]), typeof(object), false, false, true, false, false)
                },
                eventTypeComplex.PropertyDescriptors);

            properties = eventTypeNested.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(SupportBeanCombinedProps.PROPERTIES, properties);
        }

        [Test]
        public void TestGetPropertyType()
        {
            Assert.AreEqual(typeof(string), eventTypeSimple.GetPropertyType("myString"));
            Assert.IsNull(eventTypeSimple.GetPropertyType("dummy"));
        }

        [Test]
        public void TestGetUnderlyingType()
        {
            Assert.AreEqual(typeof(SupportBeanSimple), eventTypeSimple.UnderlyingType);
        }

        [Test]
        public void TestIsValidProperty()
        {
            Assert.IsTrue(eventTypeSimple.IsProperty("myString"));
            Assert.IsFalse(eventTypeSimple.IsProperty("dummy"));
        }

        [Test]
        public void TestProperties()
        {
            var nestedOne = typeof(SupportBeanCombinedProps.NestedLevOne);
            var nestedOneArr = typeof(SupportBeanCombinedProps.NestedLevOne[]);
            var nestedTwo = typeof(SupportBeanCombinedProps.NestedLevTwo);

            // test nested/combined/indexed/mapped properties
            // PropertyName                 isProperty              getXsSimpleType         hasGetter   getterValue
            IList<PropTestDesc> tests = new List<PropTestDesc>();

            tests = new List<PropTestDesc>();
            tests.Add(new PropTestDesc("SimpleProperty", true, typeof(string), true, "simple"));
            tests.Add(new PropTestDesc("dummy", false, null, false, null));
            tests.Add(new PropTestDesc("Indexed", false, null, false, null));
            tests.Add(new PropTestDesc("Indexed[1]", true, typeof(int), true, 2));
            tests.Add(new PropTestDesc("Nested", true, typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), true, objComplex.Nested));
            tests.Add(new PropTestDesc("Nested.NestedValue", true, typeof(string), true, objComplex.Nested.NestedValue));
            tests.Add(
                new PropTestDesc(
                    "Nested.NestedNested",
                    true,
                    typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNestedNested),
                    true,
                    objComplex.Nested.NestedNested));
            tests.Add(
                new PropTestDesc(
                    "Nested.NestedNested.NestedNestedValue",
                    true,
                    typeof(string),
                    true,
                    objComplex.Nested.NestedNested.NestedNestedValue));
            tests.Add(new PropTestDesc("Nested.dummy", false, null, false, null));
            tests.Add(new PropTestDesc("Mapped", false, null, false, null));
            tests.Add(new PropTestDesc("Mapped('keyOne')", true, typeof(string), true, "valueOne"));
            tests.Add(new PropTestDesc("arrayProperty", true, typeof(int[]), true, objComplex.ArrayProperty));
            tests.Add(new PropTestDesc("arrayProperty[1]", true, typeof(int), true, 20));
            tests.Add(new PropTestDesc("mapProperty('xOne')", true, typeof(string), true, "yOne"));
            tests.Add(new PropTestDesc("google('x')", false, null, false, null));
            tests.Add(new PropTestDesc("Mapped('x')", true, typeof(string), true, null));
            tests.Add(new PropTestDesc("Mapped('x').x", false, null, false, null));
            tests.Add(new PropTestDesc("mapProperty", true, typeof(IDictionary<string, object>), true, objComplex.MapProperty));
            RunTest(tests, eventTypeComplex, eventComplex);

            tests = new List<PropTestDesc>();
            tests.Add(new PropTestDesc("dummy", false, null, false, null));
            tests.Add(new PropTestDesc("myInt", true, typeof(int), true, objSimple.MyInt));
            tests.Add(new PropTestDesc("myString", true, typeof(string), true, objSimple.MyString));
            tests.Add(new PropTestDesc("dummy('a')", false, null, false, null));
            tests.Add(new PropTestDesc("dummy[1]", false, null, false, null));
            tests.Add(new PropTestDesc("dummy.Nested", false, null, false, null));
            RunTest(tests, eventTypeSimple, eventSimple);

            tests = new List<PropTestDesc>();
            tests.Add(new PropTestDesc("Indexed", false, null, false, null));
            tests.Add(new PropTestDesc("Indexed[1]", true, nestedOne, true, objCombined.GetIndexed(1)));
            tests.Add(new PropTestDesc("Indexed.Mapped", false, null, false, null));
            tests.Add(new PropTestDesc("Indexed[1].Mapped", false, null, false, null));
            tests.Add(new PropTestDesc("array", true, nestedOneArr, true, objCombined.Array));
            tests.Add(new PropTestDesc("array.Mapped", false, null, false, null));
            tests.Add(new PropTestDesc("array[0]", true, nestedOne, true, objCombined.Array[0]));
            tests.Add(new PropTestDesc("array[1].Mapped", false, null, false, null));
            tests.Add(new PropTestDesc("array[1].Mapped('x')", true, nestedTwo, true, objCombined.Array[1].GetMapped("x")));
            tests.Add(new PropTestDesc("array[1].Mapped('1mb')", true, nestedTwo, true, objCombined.Array[1].GetMapped("1mb")));
            tests.Add(new PropTestDesc("Indexed[1].Mapped('x')", true, nestedTwo, true, objCombined.GetIndexed(1).GetMapped("x")));
            tests.Add(new PropTestDesc("Indexed[1].Mapped('x').value", true, typeof(string), true, null));
            tests.Add(new PropTestDesc("Indexed[1].Mapped('1mb')", true, nestedTwo, true, objCombined.GetIndexed(1).GetMapped("1mb")));
            tests.Add(
                new PropTestDesc("Indexed[1].Mapped('1mb').value", true, typeof(string), true, objCombined.GetIndexed(1).GetMapped("1mb").Value));
            tests.Add(
                new PropTestDesc("array[1].mapprop", true, typeof(IDictionary<string, object>), true, objCombined.GetIndexed(1).GetMapprop()));
            tests.Add(
                new PropTestDesc(
                    "array[1].mapprop('1ma')",
                    true,
                    typeof(SupportBeanCombinedProps.NestedLevTwo),
                    true,
                    objCombined.Array[1].GetMapped("1ma")));
            tests.Add(
                new PropTestDesc("array[1].mapprop('1ma').value", true, typeof(string), true, "1ma0"));
            tests.Add(
                new PropTestDesc("Indexed[1].mapprop", true, typeof(IDictionary<string, object>),
                    true, objCombined.GetIndexed(1).GetMapprop()));
            RunTest(tests, eventTypeNested, eventNested);

            TryInvalidIsProperty(eventTypeComplex, "x[");
            TryInvalidIsProperty(eventTypeComplex, "dummy()");
            TryInvalidIsProperty(eventTypeComplex, "Nested.xx['a']");
            TryInvalidIsProperty(eventTypeNested, "dummy[(");
            TryInvalidIsProperty(eventTypeNested, "array[1].mapprop[x].value");
        }
    }
} // end of namespace