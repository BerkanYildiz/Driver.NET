namespace Driver.NET.DeviceIoControl
{
    using System;
    using System.IO;

    public partial class DeviceIoControl
    {
        /// <summary>
        /// Checks if the specified symbolic file exists.
        /// </summary>
        /// <param name="SymbolicName">The path of the symbolic file.</param>
        public static bool Exists(string SymbolicName)
        {
            var Handle = CreateFile(SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            var Exists = Handle != null && !Handle.IsInvalid;

            if (Handle != null)
            {
                Handle.Close();
            }

            return Exists;
        }
    }
}