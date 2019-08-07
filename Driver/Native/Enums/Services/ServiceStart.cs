namespace Driver.Native.Enums.Services
{
    /// <summary>
    /// Service start options
    /// </summary>
    public enum ServiceStart : uint
    {
        /// <summary>
        /// A device driver started by the system loader. This value is valid
        /// only for driver services.
        /// </summary>
        ServiceBootStart = 0x00000000,

        /// <summary>
        /// A device driver started by the IoInitSystem function. This value 
        /// is valid only for driver services.
        /// </summary>
        ServiceSystemStart = 0x00000001,

        /// <summary>
        /// A service started automatically by the service control manager 
        /// during system startup. For more information, see Automatically 
        /// Starting Services.
        /// </summary>         
        ServiceAutoStart = 0x00000002,

        /// <summary>
        /// A service started by the service control manager when a process 
        /// calls the StartService function. For more information, see 
        /// Starting Services on Demand.
        /// </summary>
        ServiceDemandStart = 0x00000003,

        /// <summary>
        /// A service that cannot be started. Attempts to start the service
        /// result in the error code ERROR_SERVICE_DISABLED.
        /// </summary>
        ServiceDisabled = 0x00000004,
    }
}
