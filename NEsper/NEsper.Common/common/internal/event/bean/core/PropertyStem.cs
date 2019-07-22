///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Encapsulates the event property information available after introspecting an event's class members
    ///     for getter methods.
    /// </summary>
    public class PropertyStem
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="readMethod">read method to get value</param>
        /// <param name="propertyType">type of property</param>
        public PropertyStem(
            string propertyName,
            MethodInfo readMethod,
            EventPropertyType? propertyType)
        {
            PropertyName = propertyName;
            ReadMethod = readMethod;
            PropertyType = propertyType;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">name of property, from getter method</param>
        /// <param name="accessorField">field to get value from</param>
        /// <param name="propertyType">type of property</param>
        public PropertyStem(
            string propertyName,
            FieldInfo accessorField,
            EventPropertyType propertyType)
        {
            PropertyName = propertyName;
            AccessorField = accessorField;
            PropertyType = propertyType;
        }

        /// <summary>
        ///     Return the property name, for mapped and indexed properties this is just the property name
        ///     without parentheses or brackets.
        /// </summary>
        /// <returns>property name</returns>
        public string PropertyName { get; }

        /// <summary>
        ///     Returns an enum indicating the type of property: simple, mapped, indexed.
        /// </summary>
        /// <returns>enum with property type info</returns>
        public EventPropertyType? PropertyType { get; }

        /// <summary>
        ///     Returns the read method. Can return null if the property is backed by a field..
        /// </summary>
        /// <returns>read method of null if field property</returns>
        public MethodInfo ReadMethod { get; }

        /// <summary>
        ///     Returns the accessor field. Can return null if the property is backed by a method.
        /// </summary>
        /// <returns>accessor field of null if method property</returns>
        public FieldInfo AccessorField { get; }

        /// <summary>
        ///     Returns the type of the underlying method or field of the event property.
        /// </summary>
        /// <value>return type</value>
        public Type ReturnType {
            get {
                if (ReadMethod != null) {
                    return ReadMethod.ReturnType;
                }

                return AccessorField.FieldType;
            }
        }

        /// <summary>
        /// Gets the declaring type for the property.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public Type DeclaringType {
            get {
                if (ReadMethod != null) {
                    return ReadMethod.DeclaringType;
                }

                if (AccessorField != null) {
                    return AccessorField.DeclaringType;
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns the type of the underlying method or field of the event property.
        /// </summary>
        /// <value>return type</value>
        public GenericPropertyDesc ReturnTypeGeneric {
            get {
                if (ReadMethod != null) {
                    return new GenericPropertyDesc(
                        ReadMethod.ReturnType,
                        TypeHelper.GetGenericReturnType(ReadMethod, true));
                }

                return new GenericPropertyDesc(
                    AccessorField.FieldType,
                    TypeHelper.GetGenericFieldType(AccessorField, true));
            }
        }

        public override string ToString()
        {
            return string.Format(
                "PropertyName: {0}, ReadMethod: {1}, AccessorField: {2}, EventPropertyType: {3}",
                PropertyName,
                ReadMethod,
                AccessorField,
                PropertyType);
        }

        public bool Equals(PropertyStem obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return
                Equals(obj.PropertyName, PropertyName) &&
                Equals(obj.ReadMethod, ReadMethod) &&
                Equals(obj.AccessorField, AccessorField) &&
                Equals(obj.PropertyType, PropertyType);
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(PropertyStem))
                return false;
            return Equals((PropertyStem) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                int result = (PropertyName != null ? PropertyName.GetHashCode() : 0);
                result = (result * 397) ^ (ReadMethod != null ? ReadMethod.GetHashCode() : 0);
                result = (result * 397) ^ (AccessorField != null ? AccessorField.GetHashCode() : 0);
                result = (result * 397) ^ PropertyType.GetHashCode();
                return result;
            }
        }
    }
} // end of namespace