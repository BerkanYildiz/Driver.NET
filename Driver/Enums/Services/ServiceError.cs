namespace Driver.Enums.Services
{
    /// <summary>
    /// Severity of the error, and action taken, if this service fails 
    /// to start.
    /// </summary>
    internal enum ServiceError
    {
        /// <summary>
        /// The startup program ignores the error and continues the startup
        /// operation.
        /// </summary>
        ServiceErrorIgnore = 0x00000000,

        /// <summary>
        /// The startup program logs the error in the event log but continues
        /// the startup operation.
        /// </summary>
        ServiceErrorNormal = 0x00000001,

        /// <summary>
        /// The startup program logs the error in the event log. If the 
        /// last-known-good configuration is being started, the startup 
        /// operation continues. Otherwise, the system is restarted with 
        /// the last-known-good configuration.
        /// </summary>
        ServiceErrorSevere = 0x00000002,

        /// <summary>
        /// The startup program logs the error in the event log, if possible.
        /// If the last-known-good configuration is being started, the startup
        /// operation fails. Otherwise, the system is restarted with the 
        /// last-known good configuration.
        /// </summary>
        ServiceErrorCritical = 0x00000003,
    }
}
