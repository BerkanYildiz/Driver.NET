namespace Driver.Native
{
    using System;
    using System.Runtime.InteropServices;

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
    }
}
