///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentInstrumentation : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("select 1=2 from SupportBean").UndeployAll();
        }
    }
} // end of namespace