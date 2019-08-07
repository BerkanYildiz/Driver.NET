namespace Driver
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class Log
    {
        /// <summary>
        /// Logs the specified informative message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(Type Type, string Message)
        {
            Debug.WriteLine("[ INFO  ] " + Type.Name + " : " + Message);
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(Type Type, string Message)
        {
            Debug.WriteLine("[WARNING] " + Type.Name + " : " + Message);
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Message">The message.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Type Type, string Message)
        {
            Trace.WriteLine("[ ERROR ] " + Type.Name + " : " + Message);
        }
    }
}