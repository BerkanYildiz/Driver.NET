namespace Driver.Example.Handlers.Structures
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct KernelReadRequest
    {
        /// <summary>
        /// The process identifier.
        /// </summary>
        internal int ProcessId;

        /// <summary>
        /// The address/offset of we are reading at.
        /// </summary>
        internal ulong Address;

        /// <summary>
        /// The buffer containing the response.
        /// </summary>
        internal ulong Response;

        /// <summary>
        /// The size of the buffer.
        /// </summary>
        internal ulong Size;

        /// <summary>
        /// Whether the base address must be adde to the address.
        /// </summary>
        internal bool UseBaseAddress;
    }
}