namespace Driver.NET.Loaders
{
    using System;
    using System.Security.Permissions;
    using System.ServiceProcess;

    using global::Driver.NET.Interfaces;

    [ServiceControllerPermission(SecurityAction.Demand, PermissionAccess = ServiceControllerPermissionAccess.Control)]
    public sealed class KernelServiceLoader : IDriverLoader
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether we have ownership of this service.
        /// If we don't, only the TryOpenDriver and TryStartDriver functions will be available/working.
        /// </summary>
        public bool HasOwnership
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
        /// Gets or sets the event raised when this <see cref="IDriverLoader" /> is created.
        /// </summary>
        public EventHandler OnCreated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader" /> is deleted.
        /// </summary>
        public EventHandler OnDeleted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader" /> is loaded.
        /// </summary>
        public EventHandler OnLoaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader" /> is unloaded.
        /// </summary>
        public EventHandler OnUnloaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader" /> is disposed.
        /// </summary>
        public EventHandler OnDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverLoader" /> is created.
        /// </summary>
        public bool IsCreated
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverLoader" /> is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverLoader" /> is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KernelServiceLoader"/> class.
        /// </summary>
        public KernelServiceLoader()
        {
            // ..
        }

        /// <summary>
        /// Sets the name of the service.
        /// </summary>
        /// <param name="ServiceName">Name of the service.</param>
        public void SetServiceName(string ServiceName)
        {
            if (string.IsNullOrEmpty(ServiceName))
            {
                throw new ArgumentNullException(nameof(ServiceName));
            }

            this.ServiceName = ServiceName;
        }

        /// <summary>
        /// Sets a value indicating whether we have ownership of the service.
        /// If set to false, TryCreateDriver/TryUnloadDriver/TryDeleteDriver wont be executed.
        /// </summary>
        /// <param name="HasOwnership">if set to <c>true</c>, can create/stop/delete the service.</param>
        public void SetOwnership(bool HasOwnership)
        {
            this.HasOwnership = HasOwnership;
        }

        /// <summary>
        /// Tries to create the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryCreateDriver(IDriver Driver)
        {
            if (!this.HasOwnership)
            {
                return this.TryOpenDriver(Driver);
            }
            else
            {
                // return ServiceUtils.Create(this.ServiceName, this.ServiceName, ServiceAccess.ServiceAllAccess, ServiceType.ServiceKernelDriver, ServiceStart.ServiceSystemStart, ServiceError.ServiceErrorNormal, null) != IntPtr.Zero;
            }

            throw new NotImplementedException("Creating a service is not supported yet.");
        }

        /// <summary>
        /// Tries to open the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryOpenDriver(IDriver Driver)
        {
            this.Service = new ServiceController(this.ServiceName);

            // 
            // Set the flags.
            // 

            this.IsCreated = !this.Service.ServiceHandle.IsClosed && !this.Service.ServiceHandle.IsInvalid;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (this.IsCreated)
            {
                if (this.OnCreated != null)
                {
                    try
                    {
                        this.OnCreated.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }

            return this.IsCreated;
        }

        /// <summary>
        /// Tries to load the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryLoadDriver(IDriver Driver)
        {
            if (this.IsLoaded)
            {
                return true;
            }

            // 
            // Check if the service status is already marked as running.
            // 

            if (this.Service.Status == ServiceControllerStatus.StartPending || this.Service.Status == ServiceControllerStatus.Running)
            {
                this.IsLoaded = true;
                return true;
            }

            // 
            // Start the service.
            // 

            var WasDriverStarted = false;

            try
            {
                this.Service.Start();
                WasDriverStarted = true;
            }
            catch (Exception Exception)
            {
                // ..
            }

            // 
            // Wait for it to be actually running.
            // 

            if (WasDriverStarted)
            {
                this.Service.WaitForStatus(ServiceControllerStatus.Running);
            }

            // 
            // Set the flags.
            // 

            this.IsLoaded = WasDriverStarted;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (WasDriverStarted)
            {
                if (this.OnLoaded != null)
                {
                    try
                    {
                        this.OnLoaded.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }

            return WasDriverStarted;
        }

        /// <summary>
        /// Tries to unload the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryUnloadDriver(IDriver Driver)
        {
            if (!this.IsLoaded)
            {
                return true;
            }

            // 
            // If we have ownership of the service,
            // try to stop it.
            // 

            var WasDriverStopped = false;

            if (this.HasOwnership)
            {
                // 
                // Make sure the service is not already stopped.
                // 

                if (this.Service.Status == ServiceControllerStatus.StopPending || this.Service.Status == ServiceControllerStatus.Stopped)
                {
                    WasDriverStopped = true;
                }
                else
                {
                    // 
                    // Check if the driver has an unload function.
                    // 

                    if (!this.Service.CanStop)
                    {
                        return false;
                    }

                    // 
                    // Stop the service.
                    // 

                    this.Service.Stop();

                    // 
                    // Wait for it to be actually stopped.
                    // 

                    this.Service.WaitForStatus(ServiceControllerStatus.Stopped);
                    WasDriverStopped = true;
                }
            }
            else
            {
                WasDriverStopped = true;
            }

            // 
            // Set the flags.
            // 

            this.IsLoaded = !WasDriverStopped;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (WasDriverStopped)
            {
                if (this.OnUnloaded != null)
                {
                    try
                    {
                        this.OnUnloaded.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }

            return WasDriverStopped;
        }

        /// <summary>
        /// Tries to delete the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryDeleteDriver(IDriver Driver)
        {
            if (this.IsLoaded)
            {
                throw new InvalidOperationException("Cannot delete a loaded driver.");
            }

            if (!this.IsCreated)
            {
                return true;
            }

            // 
            // If we have ownership of the service,
            // try to delete it.
            // 

            var WasDriverDeleted = false;

            if (this.HasOwnership)
            {
                throw new NotImplementedException("Deleting a service with ownership is not supported yet.");
            }
            else
            {
                WasDriverDeleted = true;
            }

            // 
            // Set the flags.
            // 

            this.IsCreated = !WasDriverDeleted;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (WasDriverDeleted)
            {
                if (this.OnDeleted != null)
                {
                    try
                    {
                        this.OnDeleted.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }

            return WasDriverDeleted;
        }

        /// <summary>
        /// Performs application-defined tasks associated with
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            // 
            // Dispose the managed instances.
            // 

            this.Service?.Dispose();

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (this.OnDisposed != null)
            {
                try
                {
                    this.OnDisposed.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ..
                }
            }
        }
    }
}