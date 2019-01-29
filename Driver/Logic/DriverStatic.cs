namespace Driver.Logic
{
    using System;
    using System.IO;

    using global::Driver.Logic.Enums;
    using global::Driver.Logic.Interfaces;
    using global::Driver.Native.Enums.Services;
    using global::Driver.Utilities;

    public partial class Driver : IDriver
    {
        /// <summary>
        /// Checks if the specified symbolic file exists.
        /// </summary>
        /// <param name="SymbolicName">Path of the symbolic file.</param>
        public static bool CanConnectTo(string SymbolicName, IoMethod IoMethod = IoMethod.IoControl)
        {
            switch (IoMethod)
            {
                case IoMethod.IoControl:
                {
                    var Handle = Native.CreateFile(SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                    var Exists = (Handle != null && !Handle.IsInvalid);

                    if (Handle != null)
                    {
                        Handle.Close();
                    }

                    return Exists;
                }

                case IoMethod.SharedMemory:
                {
                    return true;
                    break;
                }

                default:
                {
                    throw new ArgumentException();
                }
            }

            return false;
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

            if (Config.IoMethod == IoMethod.IoControl)
            {
                if (!Driver.CanConnectTo(Config.SymbolicLink))
                {
                    if (Config.DriverFile == null || !Config.DriverFile.Exists)
                    {
                        throw new FileNotFoundException("The driver file does not exist.");
                    }
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