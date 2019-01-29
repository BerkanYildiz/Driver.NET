namespace Driver.Native
{
    using System.Runtime.InteropServices;

    using Driver.Native.Enums.Memory;

    [StructLayout(LayoutKind.Sequential)] 
    public struct MemoryBasicInformation
    { 
        public ulong BaseAddress;
        public ulong AllocationBase;
        public MemoryPagePermissions AllocationProtect;
        public ulong RegionSize;
        public MemoryPageState State;
        public MemoryPagePermissions Protect;
        public MemoryPageType Type;

        /* public IntPtr EndAddress
        {
            get
            {
                return (IntPtr) ((ulong) this.BaseAddress + (ulong) this.RegionSize);
            }
        } */
    }
}
