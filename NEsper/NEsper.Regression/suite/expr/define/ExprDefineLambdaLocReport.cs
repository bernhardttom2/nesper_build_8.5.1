///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.lrreport;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
    public class ExprDefineLambdaLocReport : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            /// <summary>
            /// Regular algorithm to find separated luggage and new owner.
            /// </summary>
            var theEvent = LocationReportFactory.MakeLarge();
            var separatedLuggage = LocationReportFactory.FindSeparatedLuggage(theEvent);

            foreach (var item in separatedLuggage) {
                //log.info("Luggage that are separated (dist>20): " + item);
                var newOwner = LocationReportFactory.FindPotentialNewOwner(theEvent, item);
                //log.info("Found new owner " + newOwner);
            }

            var epl = "@Name('s0') " +
                      "expression lostLuggage {" +
                      "  lr => lr.items.where(l => l.type='L' and " +
                      "    lr.items.anyof(p => p.type='P' and p.assetId=l.assetIdPassenger and LRUtil.distance(l.location.x, l.location.y, p.location.x, p.location.y) > 20))" +
                      "}" +
                      "expression passengers {" +
                      "  lr => lr.items.where(l => l.type='P')" +
                      "}" +
                      "" +
                      "expression nearestOwner {" +
                      "  lr => lostLuggage(lr).toMap(key => key.assetId, " +
                      "     value => passengers(lr).minBy(p => LRUtil.distance(value.location.x, value.location.y, p.location.x, p.location.y)))" +
                      "}" +
                      "" +
                      "select lostLuggage(lr) as val1, nearestOwner(lr) as val2 from LocationReport lr";
            env.CompileDeploy(epl).AddListener("s0");

            var bean = LocationReportFactory.MakeLarge();
            env.SendEventBean(bean);

            var val1 = ItemArray((ICollection<Item>) env.Listener("s0").AssertOneGetNew().Get("val1"));
            Assert.AreEqual(3, val1.Length);
            Assert.AreEqual("L00000", val1[0].AssetId);
            Assert.AreEqual("L00007", val1[1].AssetId);
            Assert.AreEqual("L00008", val1[2].AssetId);

            var val2 = (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val2");
            Assert.AreEqual(3, val2.Count);
            Assert.AreEqual("P00008", ((Item) val2.Get("L00000")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00007")).AssetId);
            Assert.AreEqual("P00001", ((Item) val2.Get("L00008")).AssetId);

            env.UndeployAll();
        }

        private Item[] ItemArray(ICollection<Item> it)
        {
            return it.ToArray();
        }
    }
} // end of namespace