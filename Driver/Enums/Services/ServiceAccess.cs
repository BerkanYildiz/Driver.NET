namespace Driver.Enums.Services
{
    using System;

    /// <summary>
    /// Access to the service. Before granting the requested access, the
    /// system checks the access token of the calling process. 
    /// </summary>
    [Flags]
    internal enum ServiceAccess : uint
    {
        /// <summary>
        /// Required to call the QueryServiceConfig and 
        /// QueryServiceConfig2 functions to query the service configuration.
        /// </summary>
        ServiceQueryConfig = 0x00001,

        /// <summary>
        /// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function 
        /// to change the service configuration. Because this grants the caller 
        /// the right to change the executable file that the system runs, 
        /// it should be granted only to administrators.
        /// </summary>
        ServiceChangeConfig = 0x00002,

        /// <summary>
        /// Required to call the QueryServiceStatusEx function to ask the service 
        /// control manager about the status of the service.
        /// </summary>
        ServiceQueryStatus = 0x00004,

        /// <summary>
        /// Required to call the EnumDependentServices function to enumerate all 
        /// the services dependent on the service.
        /// </summary>
        ServiceEnumerateDependents = 0x00008,

        /// <summary>
        /// Required to call the StartService function to start the service.
        /// </summary>
        ServiceStart = 0x00010,

        /// <summary>
        ///     Required to call the ControlService function to stop the service.
        /// </summary>
        ServiceStop = 0x00020,

        /// <summary>
        /// Required to call the ControlService function to pause or continue 
        /// the service.
        /// </summary>
        ServicePauseContinue = 0x00040,

        /// <summary>
        /// Required to call the EnumDependentServices function to enumerate all
        /// the services dependent on the service.
        /// </summary>
        ServiceInterrogate = 0x00080,

        /// <summary>
        /// Required to call the ControlService function to specify a user-defined
        /// control code.
        /// </summary>
        ServiceUserDefinedControl = 0x00100,

        /// <summary>
        /// Includes STANDARD_RIGHTS_REQUIRED in addition to all access rights in this table.
        /// </summary>
        ServiceAllAccess = (AccessMask.StandardRightsRequired |
            ServiceAccess.ServiceQueryConfig |
            ServiceAccess.ServiceChangeConfig |
            ServiceAccess.ServiceQueryStatus |
            ServiceAccess.ServiceEnumerateDependents |
            ServiceAccess.ServiceStart |
            ServiceAccess.ServiceStop |
            ServiceAccess.ServicePauseContinue |
            ServiceAccess.ServiceInterrogate |
            ServiceAccess.ServiceUserDefinedControl | 
            ServiceAccess.Delete),

        GenericRead = AccessMask.StandardRightsRead |
            ServiceAccess.ServiceQueryConfig |
            ServiceAccess.ServiceQueryStatus |
            ServiceAccess.ServiceInterrogate |
            ServiceAccess.ServiceEnumerateDependents,

        GenericWrite = AccessMask.StandardRightsWrite |
            ServiceAccess.ServiceChangeConfig,

        GenericExecute = AccessMask.StandardRightsExecute |
            ServiceAccess.ServiceStart |
            ServiceAccess.ServiceStop |
            ServiceAccess.ServicePauseContinue |
            ServiceAccess.ServiceUserDefinedControl,

        /// <summary>
        /// Required to call the QueryServiceObjectSecurity or 
        /// SetServiceObjectSecurity function to access the SACL. The proper
        /// way to obtain this access is to enable the SE_SECURITY_NAME 
        /// privilege in the caller's current access token, open the handle 
        /// for ACCESS_SYSTEM_SECURITY access, and then disable the privilege.
        /// </summary>
        AccessSystemSecurity = AccessMask.AccessSystemSecurity,

        /// <summary>
        /// Required to call the DeleteService function to delete the service.
        /// </summary>
        Delete = AccessMask.Delete,

        /// <summary>
        /// Required to call the QueryServiceObjectSecurity function to query
        /// the security descriptor of the service object.
        /// </summary>
        ReadControl = AccessMask.ReadControl,

        /// <summary>
        /// Required to call the SetServiceObjectSecurity function to modify
        /// the Dacl member of the service object's security descriptor.
        /// </summary>
        WriteDac = AccessMask.WriteDac,

        /// <summary>
        /// Required to call the SetServiceObjectSecurity function to modify 
        /// the Owner and Group members of the service object's security 
        /// descriptor.
        /// </summary>
        WriteOwner = AccessMask.WriteOwner,
    }
}
