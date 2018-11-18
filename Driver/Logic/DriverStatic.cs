namespace Driver.Logic
{
    using System;
    using System.IO;

    using global::Driver.Enums;
    using global::Driver.Enums.Services;
    using global::Driver.Logic.Loaders.Interfaces;
    using global::Driver.Utilities;

    public partial class Driver : IDriver
    {
        /// <summary>
        /// Checks if the specified symbolic file exists.
        /// </summary>
        /// <param name="SymbolicName">Path of the symbolic file.</param>
        public static bool CanConnectTo(string SymbolicName)
        {
            var Handle = Native.CreateFile(SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            var Exists = (Handle != null && !Handle.IsInvalid);

            if (Handle != null)
            {
                Handle.Close();
            }

            return Exists;
        }

        /// <summary>
        /// Determines whether the specified service can be opened.
        /// </summary>
        /// <param name="ServiceName">Name of the service.</param>
        public static bool CanOpenService(string ServiceName)
        {
            var Handle = Service.Open(ServiceName, ServiceAccess.ServiceAllAccess);

            if (Handle != IntPtr.Zero)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Driver"/> with the specified argument,
        /// and loads it according to the specified <see cref="DriverLoad"/> method.
        /// </summary>
        /// <param name="Config">The configuration.</param>
        /// <exception cref="System.ArgumentNullException">Config</exception>
        /// <exception cref="System.IO.FileNotFoundException">The driver file does not exist.</exception>
        public static Driver CreateAndLoad(DriverConfig Config)
        {
            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }

            if (!Driver.CanConnectTo(Config.SymbolicLink))
            {
                if (Config.DriverFile == null || !Config.DriverFile.Exists)
                {
                    throw new FileNotFoundException("The driver file does not exist.");
                }
            }

            var DriverObject = new Driver(Config);

            try
            {
                DriverObject.Load();
            }
            catch (Exception)
            {
                // ..
            }

            return DriverObject;
        }
    }
}