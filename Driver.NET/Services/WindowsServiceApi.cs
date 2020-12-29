namespace Driver.NET.Services
{
    using System;
    using System.Runtime.InteropServices;

    public partial class WindowsService
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct UNICODE_STRING : IDisposable
        {
            internal ushort Length;
            internal ushort MaximumLength;
            private IntPtr buffer;

            internal UNICODE_STRING(string s)
            {
                this.Length = (ushort) (s.Length * 2);
                this.MaximumLength = (ushort) (this.Length + 2);
                this.buffer = Marshal.StringToHGlobalUni(s);
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(this.buffer);
                this.buffer = IntPtr.Zero;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(this.buffer);
            }
        }

        [DllImport("ntdll.dll", EntryPoint = "NtLoadDriver")]
        internal static extern unsafe ulong NtLoadDriver(
            UNICODE_STRING* ServiceName
        );

        [DllImport("ntdll.dll", EntryPoint = "NtLoadDriver")]
        internal static extern ulong NtLoadDriver(
            ref UNICODE_STRING ServiceName
        );

        [DllImport("ntdll.dll", EntryPoint = "NtUnloadDriver")]
        internal static extern unsafe ulong NtUnloadDriver(
            UNICODE_STRING* ServiceName
        );

        [DllImport("ntdll.dll", EntryPoint = "NtUnloadDriver")]
        internal static extern ulong NtUnloadDriver(
            ref UNICODE_STRING ServiceName
        );
    }
}