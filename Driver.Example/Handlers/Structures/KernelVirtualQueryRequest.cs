namespace Driver.Example.Handlers.Structures
{
    using System.Runtime.InteropServices;

    using Driver.Native;

    [StructLayout(LayoutKind.Sequential)]
    internal struct KernelVirtualQueryRequest
    {
        /// <summary>
        /// The process identifier.
        /// </summary>
        internal int ProcessId;

        /// <summary>
        /// The address/offset of we are reading at.
        /// </summary>
        internal ulong BaseAddress;

        /// <summary>
        /// The memory region.
        /// </summary>
        internal MemoryBasicInformation Result;

        /// <summary>
        /// The has been found
        /// </summary>
        internal bool HasBeenFound;
    }
}