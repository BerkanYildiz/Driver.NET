namespace Driver
{
    using System;
    using System.Diagnostics;

    internal static class Log
    {
        /// <summary>
        /// Logs the specified informative message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        [Conditional("DEBUG")]
        internal static void Info(Type Type, string Message)
        {
            Debug.WriteLine("[ INFO  ] " + Type.Name + " : " + Message);
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        [Conditional("DEBUG")]
        internal static void Warning(Type Type, string Message)
        {
            Trace.WriteLine("[WARNING] " + Type.Name + " : " + Message);
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        internal static void Error(Type Type, string Message)
        {
            Trace.WriteLine("[ ERROR ] " + Type.Name + " : " + Message);
        }

        /// <summary>
        /// Logs the specified fatal error message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        internal static void Fatal(Type Type, string Message)
        {
            Trace.WriteLine("[ FATAL ] " + Type.Name + " : " + Message);
        }
    }
}