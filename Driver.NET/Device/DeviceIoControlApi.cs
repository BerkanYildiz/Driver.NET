namespace Driver.NET.Device
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    public partial class DeviceIoControl
    {
        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern unsafe bool NtDeviceIoControl(
            SafeFileHandle Handle,
            uint IoControlCode,
            void* InputBuffer,
            int InputBufferSize,
            void* OutBuffer,
            int OutputBufferSize,
            out int ReturnedBytes,
            IntPtr Overlapped
        );

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern unsafe bool NtDeviceIoControl(
            SafeFileHandle Handle,
            uint IoControlCode,
            byte* InputBuffer,
            int InputBufferSize,
            byte* OutBuffer,
            int OutputBufferSize,
            out int ReturnedBytes,
            IntPtr Overlapped
        );

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern unsafe bool NtDeviceIoControl(
            SafeFileHandle Handle,
            uint IoControlCode,
            IntPtr InputBuffer,
            int InputBufferSize,
            IntPtr OutBuffer,
            int OutputBufferSize,
            out int ReturnedBytes,
            IntPtr Overlapped
        );

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern unsafe bool NtDeviceIoControl(
            SafeFileHandle Handle,
            uint IoControlCode,
            UIntPtr InputBuffer,
            int InputBufferSize,
            UIntPtr OutBuffer,
            int OutputBufferSize,
            out int ReturnedBytes,
            IntPtr Overlapped
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle CreateFile(
            string FileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess FileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare FileShare,
            IntPtr SecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode CreationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint FlagsAndAttributes,
            IntPtr Template
        );
    }
}