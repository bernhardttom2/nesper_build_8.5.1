///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexTraverse
    {
        public static void Traverse(
            MXCIFQuadTree<object> quadtree,
            Consumer<object> consumer)
        {
            Traverse(quadtree.Root, consumer);
        }

        public static void Traverse(
            MXCIFQuadTreeNode<object> node,
            Consumer<object> consumer)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object>) {
                MXCIFQuadTreeNodeLeaf<object> leaf = (MXCIFQuadTreeNodeLeaf<object>) node;
                TraverseData(leaf.Data, consumer);
                return;
            }

            MXCIFQuadTreeNodeBranch<object> branch = (MXCIFQuadTreeNodeBranch<object>) node;
            TraverseData(branch.Data, consumer);
            Traverse(branch.Nw, consumer);
            Traverse(branch.Ne, consumer);
            Traverse(branch.Sw, consumer);
            Traverse(branch.Se, consumer);
        }

        private static void TraverseData(
            object data,
            Consumer<object> consumer)
        {
            if (data == null) {
                return;
            }

            if (!(data is ICollection<object>)) {
                Visit(data, consumer);
                return;
            }

            ICollection<object> collection = (ICollection<object>) data;
            foreach (object datapoint in collection) {
                Visit(datapoint, consumer);
            }
        }

        private static void Visit(
            object data,
            Consumer<object> consumer)
        {
            if (data is XYWHRectangleWValue<object>) {
                consumer.Invoke(((XYWHRectangleWValue<object>) data).Value);
            }
            else if (data is XYWHRectangleMultiType) {
                XYWHRectangleMultiType multiType = (XYWHRectangleMultiType) data;
                if (multiType.Multityped is ICollection<object>) {
                    ICollection<object> collection = (ICollection<object>) multiType.Multityped;
                    foreach (object datapoint in collection) {
                        Visit(datapoint, consumer);
                    }
                }
            }
        }
    }
} // end of namespace