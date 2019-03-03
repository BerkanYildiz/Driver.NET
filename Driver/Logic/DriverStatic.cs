namespace Driver.Logic
{
    using System;
    using System.IO;

    using global::Driver.Logic.Enums;
    using global::Driver.Logic.Interfaces;
    using global::Driver.Native;

    public partial class Driver : IDriver
    {
        /// <summary>
        /// Checks if the specified symbolic file exists.
        /// </summary>
        /// <param name="SymbolicName">Path of the symbolic file.</param>
        public static bool CanConnectTo(string SymbolicName, IoMethod IoMethod = IoMethod.IoControl)
        {
            switch (IoMethod)
            {
                case IoMethod.IoControl:
                {
                    var Handle = WinApi.CreateFile(SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                    var Exists = (Handle != null && !Handle.IsInvalid);

                    if (Handle != null)
                    {
                        Handle.Close();
                    }

                    return Exists;
                }

                case IoMethod.SharedMemory:
                {
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }

            return false;
        }
    }
}