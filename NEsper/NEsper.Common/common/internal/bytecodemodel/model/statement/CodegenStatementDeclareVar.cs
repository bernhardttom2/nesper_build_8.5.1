///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementDeclareVar : CodegenStatementBase
    {
        private readonly Type _clazz;
        private readonly CodegenExpression _optionalInitializer;
        private readonly string _typeName;
        private readonly string _var;

        public CodegenStatementDeclareVar(
            Type clazz,
            string var,
            CodegenExpression optionalInitializer)
        {
            _clazz = clazz ?? throw new ArgumentException("Class cannot be null");
            _typeName = null;
            _var = var;
            _optionalInitializer = optionalInitializer;
        }

        public CodegenStatementDeclareVar(
            string typeName,
            string var,
            CodegenExpression optionalInitializer)
        {
            _clazz = null;
            _typeName = typeName ?? throw new ArgumentException("Class cannot be null");
            _var = var;
            _optionalInitializer = optionalInitializer;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            if (_clazz != null) {
                AppendClassName(builder, _clazz);
            }
            else {
                builder.Append(_typeName);
            }

            builder.Append(" ").Append(_var);
            if (_optionalInitializer != null) {
                builder.Append("=");
                _optionalInitializer.Render(builder, isInnerClass);
            }
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            if (_clazz != null) {
                classes.Add(_clazz);
            }

            _optionalInitializer?.MergeClasses(classes);
        }
    }
} // end of namespace