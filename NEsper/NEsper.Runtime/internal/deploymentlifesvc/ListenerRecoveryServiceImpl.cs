///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public class ListenerRecoveryServiceImpl : ListenerRecoveryService
    {
        public static readonly ListenerRecoveryServiceImpl INSTANCE = new ListenerRecoveryServiceImpl();

        public void Put(
            int statementId,
            string statementName,
            UpdateListener[] listeners)
        {
        }

        public IEnumerator<KeyValuePair<int, UpdateListener[]>> Listeners => EnumerationHelper.Empty<KeyValuePair<int, UpdateListener[]>>();

        public void Remove(int statementId)
        {
        }
    }
} // end of namespace