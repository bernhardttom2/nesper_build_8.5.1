///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewForgeEnv
    {
        private readonly ViewFactoryForgeArgs args;

        public ViewForgeEnv(ViewFactoryForgeArgs args)
        {
            this.args = args;
        }

        public ImportServiceCompileTime ImportServiceCompileTime => args.ImportService;

        public Configuration Configuration => args.Configuration;

        public BeanEventTypeFactory BeanEventTypeFactoryProtected => args.BeanEventTypeFactoryPrivate;

        public EventTypeCompileTimeRegistry EventTypeModuleCompileTimeRegistry =>
            args.EventTypeModuleCompileTimeRegistry;

        public Attribute[] Annotations => args.Annotations;

        public string OptionalStatementName => args.StatementName;

        public int StatementNumber => args.StatementNumber;

        public StatementCompileTimeServices StatementCompileTimeServices => args.CompileTimeServices;

        public StatementRawInfo StatementRawInfo => args.StatementRawInfo;

        public VariableCompileTimeResolver VariableCompileTimeResolver =>
            args.CompileTimeServices.VariableCompileTimeResolver;

        public string ContextName => args.StatementRawInfo.ContextName;

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver =>
            args.CompileTimeServices.EventTypeCompileTimeResolver;

        public string ModuleName => args.StatementRawInfo.ModuleName;
    }
} // end of namespace