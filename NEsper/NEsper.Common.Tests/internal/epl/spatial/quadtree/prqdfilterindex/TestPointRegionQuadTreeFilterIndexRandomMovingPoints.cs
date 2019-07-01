///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion.SupportPointRegionQuadTreeUtil;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex.SupportPointRegionQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    [TestFixture]
    public class TestPointRegionQuadTreeFilterIndexRandomMovingPoints : AbstractTestBase
    {
        [Test]
        public void TestIt()
        {
            var tools = new SupportQuadTreeToolUnique<PointRegionQuadTree<object>>(
                POINTREGION_FACTORY,
                null,
                POINTREGION_FI_ADDERUNIQUE,
                POINTREGION_FI_REMOVER,
                POINTREGION_FI_QUERIER,
                true);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 0, 0);
        }
    }
} // end of namespace