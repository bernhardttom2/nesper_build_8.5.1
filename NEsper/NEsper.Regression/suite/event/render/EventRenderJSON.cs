///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.@internal.@event.render;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.@event.render
{
    public class EventRenderJSON
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventRenderRenderSimple());
            execs.Add(new EventRenderMapAndNestedArray());
            execs.Add(new EventRenderEmptyMap());
            execs.Add(new EventRenderEnquote());
            return execs;
        }

        private static string RemoveNewline(string text)
        {
            return text.RegexReplaceAll("\\s\\s+|\\n|\\r", " ").Trim();
        }

        internal class EventRenderRenderSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var bean = new SupportBean();
                bean.TheString = "a\nc>";
                bean.IntPrimitive = 1;
                bean.IntBoxed = 992;
                bean.CharPrimitive = 'x';
                bean.EnumValue = SupportEnum.ENUM_VALUE_1;

                env.CompileDeploy("@Name('s0') select * from SupportBean").AddListener("s0");
                env.SendEventBean(bean);

                var result = env.Runtime.RenderEventService.RenderJSON("supportBean", env.Statement("s0").First());

                //System.out.println(result);
                var valuesOnly =
                    "{ \"bigDecimal\": null, \"bigInteger\": null, \"boolBoxed\": null, \"boolPrimitive\": false, \"byteBoxed\": null, \"bytePrimitive\": 0, \"charBoxed\": null, \"charPrimitive\": \"x\", \"doubleBoxed\": null, \"doublePrimitive\": 0.0, \"enumValue\": \"ENUM_VALUE_1\", \"floatBoxed\": null, \"floatPrimitive\": 0.0, \"intBoxed\": 992, \"intPrimitive\": 1, \"longBoxed\": null, \"longPrimitive\": 0, \"shortBoxed\": null, \"shortPrimitive\": 0, \"theString\": \"a\\nc>\", \"this\": { \"bigDecimal\": null, \"bigInteger\": null, \"boolBoxed\": null, \"boolPrimitive\": false, \"byteBoxed\": null, \"bytePrimitive\": 0, \"charBoxed\": null, \"charPrimitive\": \"x\", \"doubleBoxed\": null, \"doublePrimitive\": 0.0, \"enumValue\": \"ENUM_VALUE_1\", \"floatBoxed\": null, \"floatPrimitive\": 0.0, \"intBoxed\": 992, \"intPrimitive\": 1, \"longBoxed\": null, \"longPrimitive\": 0, \"shortBoxed\": null, \"shortPrimitive\": 0, \"theString\": \"a\\nc>\" } }";
                var expected = "{ \"supportBean\": " + valuesOnly + " }";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                var renderer = env.Runtime.RenderEventService.GetJSONRenderer(env.Statement("s0").EventType);
                var jsonEvent = renderer.Render("supportBean", env.Statement("s0").First());
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(jsonEvent));

                jsonEvent = renderer.Render(env.Statement("s0").First());
                Assert.AreEqual(RemoveNewline(valuesOnly), RemoveNewline(jsonEvent));

                env.UndeployAll();
            }
        }

        internal class EventRenderMapAndNestedArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from OuterMap").AddListener("s0");

                IDictionary<string, object> dataInner = new LinkedHashMap<string, object>();
                dataInner.Put("stringarr", new[] {"a", "b"});
                dataInner.Put("prop1", "");
                IDictionary<string, object> dataInnerTwo = new LinkedHashMap<string, object>();
                dataInnerTwo.Put("stringarr", new string[0]);
                dataInnerTwo.Put("prop1", "abcdef");
                IDictionary<string, object> dataOuter = new LinkedHashMap<string, object>();
                dataOuter.Put("intarr", new[] {1, 2});
                dataOuter.Put("innersimple", dataInner);
                dataOuter.Put("innerarray", new[] {dataInner, dataInnerTwo});
                dataOuter.Put("prop0", new SupportBean_A("A1"));
                env.SendEventMap(dataOuter, "OuterMap");

                var result = env.Runtime.RenderEventService.RenderJSON("outerMap", env.GetEnumerator("s0").Advance());

                //System.out.println(result);
                var expected = "{\n" +
                               "  \"outerMap\": {\n" +
                               "    \"intarr\": [1, 2],\n" +
                               "    \"innersimple\": {\n" +
                               "      \"prop1\": \"\",\n" +
                               "      \"stringarr\": [\"a\", \"b\"]\n" +
                               "    },\n" +
                               "    \"innerarray\": [{\n" +
                               "        \"prop1\": \"\",\n" +
                               "        \"stringarr\": [\"a\", \"b\"]\n" +
                               "      },\n" +
                               "      {\n" +
                               "        \"prop1\": \"abcdef\",\n" +
                               "        \"stringarr\": []\n" +
                               "      }],\n" +
                               "    \"prop0\": {\n" +
                               "      \"id\": \"A1\"\n" +
                               "    }\n" +
                               "  }\n" +
                               "}";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.UndeployAll();
            }
        }

        internal class EventRenderEmptyMap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from EmptyMapEvent");

                env.SendEventBean(new EmptyMapEvent(null));
                var result = env.Runtime.RenderEventService.RenderJSON("outer", env.GetEnumerator("s0").Advance());
                var expected = "{ \"outer\": { \"props\": null } }";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.SendEventBean(new EmptyMapEvent(Collections.GetEmptyMap<string, string>()));
                result = env.Runtime.RenderEventService.RenderJSON("outer", env.GetEnumerator("s0").Advance());
                expected = "{ \"outer\": { \"props\": {} } }";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.SendEventBean(new EmptyMapEvent(Collections.SingletonMap("a", "b")));
                result = env.Runtime.RenderEventService.RenderJSON("outer", env.GetEnumerator("s0").Advance());
                expected = "{ \"outer\": { \"props\": { \"a\": \"b\" } } }";
                Assert.AreEqual(RemoveNewline(expected), RemoveNewline(result));

                env.UndeployAll();
            }
        }

        internal class EventRenderEnquote : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[][] testdata = {
                    new[] {"\t", "\"\\t\""},
                    new[] {"\n", "\"\\n\""},
                    new[] {"\r", "\"\\r\""},
                    new[] {Convert.ToString((char) 0), "\"\\u0000\""}
                };

                for (var i = 0; i < testdata.Length; i++) {
                    var buf = new StringBuilder();
                    OutputValueRendererJSONString.Enquote(testdata[i][0], buf);
                    Assert.AreEqual(testdata[i][1], buf.ToString());
                }
            }
        }

        public class EmptyMapEvent
        {
            private readonly IDictionary<string, string> props;

            public EmptyMapEvent(IDictionary<string, string> props)
            {
                this.props = props;
            }

            public IDictionary<string, string> GetProps()
            {
                return props;
            }
        }
    }
} // end of namespace