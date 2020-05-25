namespace Driver.NET.Example.Structures
{
    using System.Runtime.InteropServices;

    using global::Driver.NET.Example.Enums;

    [StructLayout(LayoutKind.Sequential)]
    public struct STORAGE_DEVICE_DESCRIPTOR
    {
        public int Version;
        public int Size;
        public byte DeviceType;
        public byte DeviceTypeModifier;
        [MarshalAs(UnmanagedType.U1)] public bool RemovableMedia;
        [MarshalAs(UnmanagedType.U1)] public bool CommandQueueing;
        public int VendorIdOffset;
        public int ProductIdOffset;
        public int ProductRevisionOffset;
        public int SerialNumberOffset;
        public STORAGE_BUS_TYPE BusType;
        public int RawPropertiesLength;

        // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x16)]
        // public byte[] RawDeviceProperties;

        public byte RawDeviceProperties1;
        public byte RawDeviceProperties2;
        public byte RawDeviceProperties3;
        public byte RawDeviceProperties4;
        public byte RawDeviceProperties5;
        public byte RawDeviceProperties6;
        public byte RawDeviceProperties7;
        public byte RawDeviceProperties8;
        public byte RawDeviceProperties9;
        public byte RawDeviceProperties10;
        public byte RawDeviceProperties11;
        public byte RawDeviceProperties12;
        public byte RawDeviceProperties13;
        public byte RawDeviceProperties14;
        public byte RawDeviceProperties15;
        public byte RawDeviceProperties16;
    }
}
