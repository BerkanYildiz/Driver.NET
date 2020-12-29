namespace Driver.NET.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;

    using Microsoft.Win32;

    public partial class WindowsService
    {
        /// <summary>
        /// The full registry path for the 'Services' key, where Windows Services configurations are stored.
        /// </summary>
        private const string RegistryPathForServices = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services";

        /// <summary>
        /// The relative (to local machine) registry path for the 'Services' key, where Windows Services configurations are stored.
        /// </summary>
        private const string RegistryPathForServicesRelative = @"SYSTEM\CurrentControlSet\Services";

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsService"/> class using a service name.
        /// </summary>
        /// <param name="ServiceName">The name of the service.</param>
        public static WindowsService FromServiceName(string ServiceName)
        {
            return WindowsService.FromRegistryPath(Path.Combine(RegistryPathForServices, ServiceName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsService"/> class using a registry path.
        /// </summary>
        /// <param name="RegistryPath">The registry path of the service.</param>
        public static WindowsService FromRegistryPath(string RegistryPath)
        {
            // 
            // Check if the registry path is null or empty.
            // 

            if (string.IsNullOrEmpty(RegistryPath))
            {
                throw new ArgumentNullException(nameof(RegistryPath), "The registry path is null or empty.");
            }

            // 
            // Check the format of the registry path.
            //   - Format: HKEY_LOCAL_MACHINE
            // 

            if (!RegistryPath.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                throw new ArgumentException("The registry path does not start with the local machine hive base path.");
            }

            // 
            // Convert all forward slashes '/' to backward slashes '\' and return.
            // 

            return new WindowsService
            {
                RegistryPath = RegistryPath.Replace('/', '\\')
            };
        }

        /// <summary>
        /// Determines whether the given service is installed (present in registry) on the local machine.
        /// </summary>
        /// <param name="ServiceName">The name of the service.</param>
        /// <returns>
        ///   <c>true</c> if the specified service is installed; otherwise, <c>false</c>.
        /// </returns>
        public static bool Exists(string ServiceName)
        {
            // 
            // Open the 'Services' key in registry.
            // 

            var ServicesKey = Registry.LocalMachine.OpenSubKey(RegistryPathForServicesRelative, RegistryRights.ReadKey);

            if (ServicesKey == null)
            {
                throw new Exception("Failed to open the services registry key.");
            }

            // 
            // Retrieve all sub-keys available.
            // 

            var ServiceNames = ServicesKey.GetSubKeyNames();

            // 
            // Check if the service we are searching for is in the list.
            // 

            var Exists = ServiceNames.Any(T => string.Equals(T, ServiceName, StringComparison.InvariantCultureIgnoreCase));

            // 
            // Close the registry key and return.
            // 

            ServicesKey.Close();
            return Exists;
        }
    }
}