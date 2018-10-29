namespace Driver.Utilities
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using System.ServiceProcess;

    using Driver.Enums.Services;

    using Microsoft.Win32;

    using ServiceType = Driver.Enums.Services.ServiceType;

    internal static class Service
    {
        /// <summary>
        /// Creates the specified service.
        /// </summary>
        /// <param name="ServiceName">Name of the service.</param>
        /// <param name="DisplayName">The display name.</param>
        /// <param name="ServiceAccess">The service access.</param>
        /// <param name="ServiceType">Type of the service.</param>
        /// <param name="ServiceStart">The service start.</param>
        /// <param name="ServiceError">The service error.</param>
        /// <param name="File">The file.</param>
        internal static IntPtr Create(string ServiceName, string DisplayName, ServiceAccess ServiceAccess, ServiceType ServiceType, ServiceStart ServiceStart, ServiceError ServiceError, FileInfo File)
        {
            IntPtr ServiceManager = Native.OpenSCManager(null, null, (uint) ScmAccess.ScManagerAllAccess);

            if (ServiceManager == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            IntPtr Service = Native.CreateService(
                ServiceManager,
                ServiceName,
                DisplayName,
                (uint) ServiceAccess,
                (uint) ServiceType,
                (uint) ServiceStart,
                (uint) ServiceError,
                File.FullName,
                null, null, null, null, null
            );

            Native.CloseServiceHandle(ServiceManager);

            if (Service == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            // Native.CloseServiceHandle(Service);

            return Service;
        }

        /// <summary>
        /// Creates or opens the specified service.
        /// </summary>
        /// <param name="ServiceName">Name of the service.</param>
        /// <param name="DisplayName">The display name.</param>
        /// <param name="ServiceAccess">The service access.</param>
        /// <param name="ServiceType">Type of the service.</param>
        /// <param name="ServiceStart">The service start.</param>
        /// <param name="ServiceError">The service error.</param>
        /// <param name="File">The file.</param>
        internal static IntPtr CreateOrOpen(string ServiceName, string DisplayName, ServiceAccess ServiceAccess, ServiceType ServiceType, ServiceStart ServiceStart, ServiceError ServiceError, FileInfo File)
        {
            var Handle = Service.Create(ServiceName, DisplayName, ServiceAccess, ServiceType, ServiceStart, ServiceError, File);

            if (Handle == IntPtr.Zero)
            {
                // TODO
            }

            return Handle;
        }

        /// <summary>
        /// Checks if a service exist using the specified comparer.
        /// </summary>
        /// <param name="Comparer">The comparer.</param>
        internal static bool Exists(string ServiceName, Func<ServiceController, bool> Comparer)
        {
            var Services = ServiceController.GetServices();

            if (Services.Any(Comparer))
            {
                return true;
            }

            using (var Regedit = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\", RegistryRights.EnumerateSubKeys))
            {
                var Names = Regedit.GetSubKeyNames();

                if (Names.Any(Name => Name == ServiceName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes the specified service.
        /// </summary>
        /// <param name="Handle">The handle.</param>
        /// <param name="Force">Whether the deletion is forced.</param>
        /// <exception cref="ArgumentException">Handle is invalid at Delete(Handle). - Handle</exception>
        internal static bool Delete(IntPtr Handle, bool Forced = false)
        {
            if (Handle == IntPtr.Zero)
            {
                throw new ArgumentException("Handle is invalid at Delete(Handle).", nameof(Handle));
            }

            if (!Native.DeleteService(Handle))
            {
                return false;
            }

            Native.CloseServiceHandle(Handle);

            return true;
        }
    }
}
