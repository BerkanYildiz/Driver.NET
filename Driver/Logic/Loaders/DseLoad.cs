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
        /// Gets a value indicating whether this driver is running.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverLoad"/> is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the disposed event.
        /// </summary>
        public EventHandler Disposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the driver.
        /// </summary>
        private Driver Driver
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the service handle.
        /// </summary>
        private IntPtr ServiceHandle
        {
            get;
            set;
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
            if (this.IsCreated)
            {
                throw new Exception("Driver is already created.");
            }

            this.Driver = Driver;

            if (Driver == null)
            {
                throw new ArgumentNullException(nameof(Driver), "Driver is null.");
            }

            this.ServiceHandle = Utilities.Service.Create(Driver.ServiceName, Driver.ServiceName, ServiceAccess.ServiceAllAccess, ServiceType.ServiceKernelDriver, ServiceStart.ServiceDemandStart, ServiceError.ServiceErrorNormal, Driver.DriverFile);

            if (this.ServiceHandle == IntPtr.Zero)
            {
                return false;
            }

            this.Service = new ServiceController(Driver.ServiceName);

            if (this.Service.CanStop)
            {
                try
                {
                    this.Service.Stop();
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            else
            {
                if (this.Service.Status != ServiceControllerStatus.Stopped)
                {
                    return false;
                }
            }

            try
            {
                this.Service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                return false;
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
                throw new Exception("Driver is not created.");
            }

            if (this.IsLoaded)
            {
                if (this.Service.Status != ServiceControllerStatus.Running)
                {
                    return false;
                }

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
                return false;
            }

            DSEFix.EnableSecurity();

            try
            {
                this.Service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
            }
            catch (System.ServiceProcess.TimeoutException)
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
            if (!this.IsLoaded)
            {
                if (this.Service.Status == ServiceControllerStatus.Stopped || this.Service.Status == ServiceControllerStatus.StopPending)
                {
                    return true;
                }

                return false;
            }

            if (this.Service != null)
            {
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
                        this.Service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                    }
                    catch (TimeoutException)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Deletes the specified driver.
        /// </summary>
        public bool DeleteDriver()
        {
            if (this.Service != null)
            {
                if (this.Service.Status != ServiceControllerStatus.Stopped)
                {
                    this.Service.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                this.Service.Dispose();
            }

            if (this.ServiceHandle != IntPtr.Zero)
            {
                if (!Utilities.Service.Delete(this.ServiceHandle))
                {
                    // return false;
                }

                this.ServiceHandle = IntPtr.Zero;
            }

            this.IsCreated  = false;
            this.IsLoaded   = false;
            this.IsRunning  = false;

            return true;
        }

        /// <summary>
        /// Exécute les tâches définies par l'application associées à la
        /// libération ou à la redéfinition des ressources non managées.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            if (this.Disposed != null)
            {
                try
                {
                    this.Disposed.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ..
                }
            }
        }
    }
}
