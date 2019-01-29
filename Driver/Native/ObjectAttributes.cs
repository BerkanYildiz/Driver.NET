namespace Driver.Native
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_ATTRIBUTES : IDisposable
    {
        public int Length;
        public IntPtr RootDirectory;
        private IntPtr objectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;

        public OBJECT_ATTRIBUTES(string name, uint attrs)
        {
            this.Length = 0;
            this.RootDirectory = IntPtr.Zero;
            this.objectName = IntPtr.Zero;
            this.Attributes = attrs;
            this.SecurityDescriptor = IntPtr.Zero;
            this.SecurityQualityOfService = IntPtr.Zero;

            this.Length = Marshal.SizeOf(this);
            this.ObjectName = new UNICODE_STRING(name);
        }

        public UNICODE_STRING ObjectName
        {
            get
            {
                return (UNICODE_STRING)Marshal.PtrToStructure(
                    this.objectName, typeof(UNICODE_STRING));
            }

            set
            {
                bool fDeleteOld = this.objectName != IntPtr.Zero;
                if (!fDeleteOld)
                    this.objectName = Marshal.AllocHGlobal(Marshal.SizeOf(value));
                Marshal.StructureToPtr(value, this.objectName, fDeleteOld);
            }
        }

        public void Dispose()
        {
            if (this.objectName != IntPtr.Zero)
            {
                Marshal.DestroyStructure(this.objectName, typeof(UNICODE_STRING));
                Marshal.FreeHGlobal(this.objectName);
                this.objectName = IntPtr.Zero;
            }
        }
    }
}
