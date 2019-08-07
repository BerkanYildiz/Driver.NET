namespace Driver.Native.Enums.Services
{
    using System;

    /// <summary>
    /// Service types.
    /// </summary>
    [Flags]
    public enum ServiceType : uint
    {
        /// <summary>
        /// Driver service.
        /// </summary>
        ServiceKernelDriver = 0x00000001,

        /// <summary>
        /// File system driver service.
        /// </summary>
        ServiceFileSystemDriver = 0x00000002,

        /// <summary>
        /// Service that runs in its own process.
        /// </summary>
        ServiceWin32OwnProcess = 0x00000010,

        /// <summary>
        /// Service that shares a process with one or more other services.
        /// </summary>
        ServiceWin32ShareProcess = 0x00000020,

        /// <summary>
        /// The service can interact with the desktop.
        /// </summary>
        ServiceInteractiveProcess = 0x00000100,
    }
}