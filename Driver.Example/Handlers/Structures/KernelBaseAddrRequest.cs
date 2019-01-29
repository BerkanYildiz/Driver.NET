namespace Driver.Example.Handlers.Structures
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct KernelBaseAddrRequest
    {
        /// <summary>
        /// The process identifier.
        /// </summary>
        internal int ProcessId;

        /// <summary>
        /// The process main module base address.
        /// </summary>
        internal ulong BaseAddress;
    }
}