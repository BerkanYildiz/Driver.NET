namespace Driver.NET
{
    using System;
    using System.Diagnostics;

    using global::Driver.NET.Interfaces;

    public partial class Driver : IDriver
    {
        /// <summary>
        /// Gets the IO requests handler.
        /// </summary>
        public IDriverIo IO
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the driver loader.
        /// </summary>
        public IDriverLoader Loader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public DriverConfig Config
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the event raised when this driver is loaded.
        /// </summary>
        public EventHandler OnLoaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this driver is unloaded.
        /// </summary>
        public EventHandler OnUnloaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this driver instance is disposed.
        /// </summary>
        public EventHandler OnDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Driver"/> is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Driver"/> is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Driver"/> class.
        /// </summary>
        public Driver()
        {
            // ..
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Driver"/> class.
        /// </summary>
        /// <param name="DriverConfig">The driver configuration.</param>
        /// <param name="DriverLoader">The driver loader.</param>
        /// <param name="DriverIo">The driver IO requests handler.</param>
        public Driver(DriverConfig DriverConfig, IDriverLoader DriverLoader, IDriverIo DriverIo)
        {
            if (DriverConfig == null)
            {
                throw new ArgumentNullException(nameof(DriverConfig));
            }

            if (DriverLoader == null)
            {
                throw new ArgumentNullException(nameof(DriverLoader));
            }

            if (DriverIo == null)
            {
                throw new ArgumentNullException(nameof(DriverIo));
            }

            this.Setup(DriverConfig, DriverLoader, DriverIo);
        }

        /// <summary>
        /// Setups the driver.
        /// </summary>
        /// <param name="DriverConfig">The driver configuration.</param>
        /// <param name="DriverLoader">The driver loader.</param>
        /// <param name="DriverIo">The driver IO requests handler.</param>
        public void Setup(DriverConfig DriverConfig, IDriverLoader DriverLoader, IDriverIo DriverIo)
        {
            if (DriverConfig == null)
            {
                throw new ArgumentNullException(nameof(DriverConfig));
            }

            if (DriverLoader == null)
            {
                throw new ArgumentNullException(nameof(DriverLoader));
            }

            if (DriverIo == null)
            {
                throw new ArgumentNullException(nameof(DriverIo));
            }

            this.Config = DriverConfig;
            this.Loader = DriverLoader;
            this.IO = DriverIo;
        }

        /// <summary>
        /// Tries to load the driver.
        /// </summary>
        public bool TryLoad()
        {
            // 
            // Throw an exception if the driver is disposed.
            // 

            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Driver));
            }

            // 
            // Return immediately if the driver is already loaded.
            // 

            if (this.IsLoaded)
            {
                return true;
            }

            // 
            // Make sure the driver is created.
            // 

            if (!this.Loader.TryCreateDriver(this))
            {
                Debug.WriteLine("Failed to create the driver.");
                return false;
            }

            // 
            // Call the developer's custom load function.
            // 

            var HasDriverLoaded = false;

            try
            {
                HasDriverLoaded = this.Loader.TryLoadDriver(this);
            }
            catch (Exception Exception)
            {
                Debug.WriteLine("Failed to load the driver.");
                Debug.WriteLine(Exception.Message);
            }

            // 
            // If the driver has failed to load..
            // 

            if (!HasDriverLoaded)
            {
                try
                {
                    this.TryUnload();
                }
                catch (Exception Exception)
                {
                    Debug.WriteLine("Failed to unload the driver.");
                    Debug.WriteLine(Exception.Message);
                }
            }

            // 
            // Set the flag and execute the handlers subscribed to the event.
            // 

            this.IsLoaded = HasDriverLoaded;

            if (HasDriverLoaded)
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

            return HasDriverLoaded;
        }

        /// <summary>
        /// Tries to unload the driver.
        /// </summary>
        public bool TryUnload()
        {
            // 
            // Throw an exception if the driver is disposed.
            // 

            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Driver));
            }

            // 
            // Return immediately if the driver is already unloaded.
            // 

            if (!this.IsLoaded)
            {
                return true;
            }

            // 
            // Call the developer's custom unload function.
            // 

            var HasDriverUnloaded = false;

            try
            {
                HasDriverUnloaded = this.Loader.TryUnloadDriver(this);
            }
            catch (Exception Exception)
            {
                Debug.WriteLine("Failed to unload the driver.");
                Debug.WriteLine(Exception.Message);
            }

            // 
            // If the driver has unloaded, we can delete it.
            // 

            if (HasDriverUnloaded)
            {
                if (!this.Loader.TryDeleteDriver(this))
                {
                    Debug.WriteLine("Failed to delete the driver.");
                }
            }
            else
            {
                HasDriverUnloaded = true;
            }

            // 
            // Set the flag and execute the handlers subscribed to the event.
            // 

            this.IsLoaded = !HasDriverUnloaded;

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

            return HasDriverUnloaded;
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
            // Make sure the driver is unloaded first.
            // We don't care if it failed to unload.
            // 

            if (this.IsLoaded)
            {
                try
                {
                    this.TryUnload();
                }
                catch (Exception)
                {
                    // ..
                }
            }

            // 
            // Dispose the managed instances.
            // 

            if (this.Loader != null)
            {
                this.Loader.Dispose();
            }

            if (this.IO != null)
            {
                this.IO.Dispose();
            }

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
                    // ...
                }
            }
        }
    }
}