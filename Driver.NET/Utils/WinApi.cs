namespace Driver.NET.Utils
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    internal static class WinApi
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle CreateFile(
            string FileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess FileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare FileShare,
            IntPtr SecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode CreationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint FlagsAndAttributes,
            IntPtr Template);
    }
}
