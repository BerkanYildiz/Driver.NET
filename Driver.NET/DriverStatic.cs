namespace Driver.NET
{
    using System;
    using System.IO;

    using global::Driver.NET.Utils;

    public partial class Driver
    {
        /// <summary>
        /// Checks if the specified symbolic file exists.
        /// </summary>
        /// <param name="SymbolicName">The path of the symbolic file.</param>
        public static bool CanConnectTo(string SymbolicName)
        {
            var Handle = WinApi.CreateFile(SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            var Exists = Handle != null && !Handle.IsInvalid;

            if (Handle != null)
            {
                Handle.Close();
            }

            return Exists;
        }
    }
}