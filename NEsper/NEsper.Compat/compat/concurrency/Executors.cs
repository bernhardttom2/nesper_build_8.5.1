///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading.Tasks;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// Class that provides access to threadPool like services.  This class exists to
    /// provide an easier bridge between the CLR thread pool and the JVM thread pool
    /// mechanisms.
    /// </summary>
    /// 
    public class Executors
    {
        /// <summary>
        /// Creates a new thread pool and returns the executor.  Ours does
        /// nothing as we use the CLR thread pool.
        /// </summary>
        /// <returns></returns>
        public static DefaultExecutorService DefaultExecutor()
        {
            return new DefaultExecutorService(Task.Factory);
        }

        /// <summary>
        /// Creates a new thread pool and returns the executor.
        /// </summary>
        /// <returns></returns>
        public static DefaultExecutorService NewSingleThreadExecutor()
        {
            return new DefaultExecutorService(
                new TaskFactory(
                    new SingleThreadedTaskScheduler()));
        }
        
        /// <summary>
        /// Creates a scheduled executor service that uses the thread pool for task execution.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static IScheduledExecutorService DefaultScheduledExecutorService()
        {
            throw new System.NotImplementedException();
        }
    }
}