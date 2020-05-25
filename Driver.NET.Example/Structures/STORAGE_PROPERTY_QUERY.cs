namespace Driver.NET.Example.Structures
{
    using System.Runtime.InteropServices;

    using global::Driver.NET.Example.Enums;

    [StructLayout(LayoutKind.Sequential)]
    public struct STORAGE_PROPERTY_QUERY
    {
        public STORAGE_PROPERTY_ID PropertyId;
        public STORAGE_QUERY_TYPE QueryType;

        // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        // public byte[] AdditionalParameters;
        public byte AdditionalParameters;
    }
}
