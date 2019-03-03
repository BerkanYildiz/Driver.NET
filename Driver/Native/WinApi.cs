namespace Driver.Native
{
    using System;
    using System.IO;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    using Microsoft.Win32.SafeHandles;

    internal static class WinApi
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint NtCreateSection(
            ref IntPtr SectionHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes,
            ref ulong MaximumSize,
            uint SectionPageProtection,
            uint AllocationAttributes,
            IntPtr FileHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint NtOpenSection(
            ref IntPtr SectionHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes);

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint NtMapViewOfSection(
            IntPtr SectionHandle,
            IntPtr ProcessHandle,
            ref IntPtr BaseAddress,
            UIntPtr ZeroBits,
            UIntPtr CommitSize,
            out ulong SectionOffset,
            out uint ViewSize,
            uint InheritDisposition,
            uint AllocationType,
            uint Win32Protect);

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint NtCreateEvent(
            ref IntPtr EventHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes,
            uint EventType,
            bool InitialState);

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint NtOpenEvent(
            ref IntPtr EventHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes);

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint NtClose(
            IntPtr Handle);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        internal static extern void CopyMemory(
            IntPtr Destination,
            IntPtr Source,
            uint Length);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibrary(
            string dllToLoad);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(
            IntPtr hModule,
            string procedureName);
        
        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(
            IntPtr hModule);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr OpenSCManager(string MachineName, string DatabaseName, uint DesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateService(
            IntPtr              ServiceManager,
            string              ServiceName,
            string              DisplayName,
            uint                DesiredAccess,
            uint                ServiceType,
            uint                StartType,
            uint                ErrorControl,
            string              BinaryPathName,
            string              LoadOrderGroup,
            string              TagId,
            string              Dependencies,
            string              ServiceStartName,
            string              Password
        );

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr OpenService(IntPtr ServiceManager, string ServiceName, uint DesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteService(IntPtr Service);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseServiceHandle(IntPtr Handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr Handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle CreateFile(
                                            string              FileName,
            [MarshalAs(UnmanagedType.U4)]   FileAccess          FileAccess,
            [MarshalAs(UnmanagedType.U4)]   FileShare           FileShare,
                                            IntPtr              SecurityAttributes,
            [MarshalAs(UnmanagedType.U4)]   FileMode            CreationDisposition,
            [MarshalAs(UnmanagedType.U4)]   uint                FlagsAndAttributes,
                                            IntPtr              Template);

        internal static uint CtlCode(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return (((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method));
        } 
    }
}
