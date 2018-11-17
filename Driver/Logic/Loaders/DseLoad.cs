namespace Driver.Logic.Loaders
{
    using System;
    using System.ComponentModel;
    using System.ServiceProcess;

    using global::Driver.Enums.Services;
    using global::Driver.Logic.Loaders.Interfaces;
    using global::Driver.Utilities;

    using ServiceType       = global::Driver.Enums.Services.ServiceType;
    using TimeoutException  = System.TimeoutException;

    internal sealed class DseLoad : IDriverLoad
    {
        /// <summary>
        /// Gets a value indicating whether this driver is created.
        /// </summary>
        public bool IsCreated
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this driver is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the driver.
        /// </summary>
        public Driver Driver
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service handle.
        /// </summary>
        public IntPtr ServiceHandle
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service associated to this driver.
        /// </summary>
        public ServiceController Service
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates the specified driver.
        /// </summary>
        public bool CreateDriver(Driver Driver)
        {
            var Config = Driver.Config;

            if (this.IsCreated)
            {
                throw new Exception("Service is already created");
            }

            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }

            this.Driver = Driver;

            if (Driver == null)
            {
                throw new ArgumentNullException(nameof(Driver), "Driver is null");
            }

            this.ServiceHandle = Utilities.Service.CreateOrOpen(Config.ServiceName, Config.ServiceName, ServiceAccess.ServiceAllAccess, ServiceType.ServiceKernelDriver, ServiceStart.ServiceDemandStart, ServiceError.ServiceErrorNormal, Config.DriverFile);

            if (this.ServiceHandle == IntPtr.Zero)
            {
                return false;
            }

            this.Service = new ServiceController(Config.ServiceName);

            if (this.Service.Status != ServiceControllerStatus.Stopped)
            {
                if (!this.StopDriver())
                {
                    return false;
                }
            }

            this.IsCreated = true;

            return true;
        }

        /// <summary>
        /// Loads the specified driver.
        /// </summary>
        public bool LoadDriver()
        {
            if (!this.IsCreated)
            {
                throw new Exception("Service is not created.");
            }

            if (this.IsLoaded)
            {
                return true;
            }

            DSEFix.DisableSecurity();

            try
            {
                this.Service.Start();
            }
            catch (InvalidOperationException Exception)
            {
                DSEFix.EnableSecurity();
                return false;
            }
            catch (Win32Exception Exception)
            {
                DSEFix.EnableSecurity();

                if (Exception.Message.Contains("signature"))
                {
                    Log.Warning(typeof(DseLoad), "The driver is not signed, unable to load it using the service manager.");
                }

                return false;
            }

            try
            {
                this.Service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                Log.Error(typeof(DseLoad), "Failed to start the service in 10 seconds.");
            }

            DSEFix.EnableSecurity();

            if (this.Service.Status != ServiceControllerStatus.Running)
            {
                return false;
            }

            this.IsLoaded = true;

            return true;
        }

        /// <summary>
        /// Stops the specified driver.
        /// </summary>
        public bool StopDriver()
        {
            if (!this.IsCreated)
            {
                throw new Exception("Service is not created.");
            }

            if (!this.IsLoaded)
            {
                return true;
            }

            if (this.Service.CanStop || this.Service.CanShutdown)
            {
                try
                {
                    this.Service.Stop();
                }
                catch (InvalidOperationException)
                {
                    return false;
                }

                this.IsLoaded = false;

                try
                {
                    this.Service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                }
                catch (TimeoutException)
                {
                    Log.Error(typeof(DseLoad), "Failed to stop the service in 10 seconds.");
                }
            }

            if (this.Service.Status != ServiceControllerStatus.Stopped)
            {
                return false;
            }

            this.IsLoaded = false;

            return true;
        }

        /// <summary>
        /// Deletes the specified driver.
        /// </summary>
        public bool DeleteDriver()
        {
            if (!this.IsCreated)
            {
                throw new Exception("Service is not created.");
            }

            if (this.IsLoaded)
            {
                if (!this.StopDriver())
                {
                    return false;
                }
            }

            if (this.Service != null)
            {
                this.Service.Dispose();
            }

            if (this.ServiceHandle != IntPtr.Zero)
            {
                if (!Utilities.Service.Delete(this.ServiceHandle))
                {
                    Log.Error(typeof(DseLoad), "Unable to delete the service using the native api.");
                }

                this.ServiceHandle = IntPtr.Zero;
            }

            this.IsCreated  = false;

            return true;
        }
    }
}
