///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotEvalParamLambda : ExprDotEvalParam
    {
        public ExprDotEvalParamLambda(
            int parameterNum,
            ExprNode body,
            ExprForge bodyEvaluator,
            int streamCountIncoming,
            IList<string> goesToNames,
            EventType[] goesToTypes)
            : base(parameterNum, body, bodyEvaluator)
        {
            StreamCountIncoming = streamCountIncoming;
            GoesToNames = goesToNames;
            GoesToTypes = goesToTypes;
        }

        public int StreamCountIncoming { get; }

        public IList<string> GoesToNames { get; }

        public EventType[] GoesToTypes { get; }
    }
} // end of namespace