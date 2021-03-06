﻿using System.Threading;

namespace com.espertech.esper.compat
{
    public class NamespaceGenerator
    {
        private static long _generation;

        /// <summary>
        /// Returns the identity counter for the type.
        /// </summary>
        public static long Generation => _generation;

        /// <summary>
        /// Returns a new namespace.  Namespace is guaranteed to be unique from
        /// other namespaces generated by this class within this AppDomain.
        /// </summary>
        /// <returns></returns>
        public static string Create()
        {
            var generation = Interlocked.Increment(ref _generation);
            return $"namespace_{generation}";
        }
    }
}