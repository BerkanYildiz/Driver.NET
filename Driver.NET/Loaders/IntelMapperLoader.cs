namespace ACore.Driver.Loaders
{
    using System;

    using ACore.Driver.Interfaces;
    using ACore.Driver.IO;
    using ACore.DriverMapper;

    public sealed class IntelMapperLoader : IDriverLoader
    {
        /// <summary>
        /// Gets the shared memory configuration.
        /// </summary>
        public SharedMemoryConfig Config
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the driver buffer.
        /// </summary>
        private byte[] DriverBuffer
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
        /// Initializes a new instance of the <see cref="IntelMapperLoader"/> class.
        /// </summary>
        public IntelMapperLoader()
        {
            this.OnLoaded += (Sender, Args) =>
            {
                Log.Debug(this, "TODO: Load the driver.");
            };

            this.OnUnloaded += (Sender, Args) =>
            {
                Log.Debug(this, "TODO: Unload the driver.");
            };

            this.OnDisposed += (Sender, Args) =>
            {
                Log.Debug(this, "TODO: Dispose the driver.");
            };
        }

        /// <summary>
        /// Setups the shared memory system.
        /// </summary>
        /// <param name="Config">The configuration.</param>
        public void SetupSharedMemory(SharedMemoryConfig Config)
        {
            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }

            this.Config = Config;
        }

        /// <summary>
        /// Setups the driver.
        /// </summary>
        /// <param name="DriverBuffer">The driver buffer.</param>
        public void SetupDriver(byte[] DriverBuffer)
        {
            if (DriverBuffer == null || DriverBuffer.Length == 0)
            {
                throw new ArgumentNullException(nameof(DriverBuffer));
            }

            this.DriverBuffer = DriverBuffer;
        }

        /// <summary>
        /// Tries to create the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryCreateDriver(IDriver Driver)
        {
            return true;
        }

        /// <summary>
        /// Tries to open the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryOpenDriver(IDriver Driver)
        {
            throw new NotImplementedException("Opening a manual mapped driver is not supported.");
        }

        /// <summary>
        /// Tries to load the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryLoadDriver(IDriver Driver)
        {
            // 
            // Tries to load the exploitable intel driver.
            // 

            var IqvDriverMapper = new DriverMapper();

            if (!IqvDriverMapper.Setup())
            {
                IqvDriverMapper.Dispose();
                Log.Error(this, "Failed to setup the driver mapper.");
                return false;
            }

            // 
            // Tries to map our driver using the intel driver.
            // 

            var WasDriverMapped = false;

            if (this.Config != null)
            {
                WasDriverMapped = IqvDriverMapper.MapDriver(this.DriverBuffer, this.Config.FirstEventName, this.Config.SecondEventName, this.Config.MapAddress);
            }
            else
            {
                WasDriverMapped = IqvDriverMapper.MapDriver(this.DriverBuffer);
            }

            // 
            // If it failed to map the driver...
            // 

            if (!WasDriverMapped)
            {
                Log.Error(this, "Failed to map the driver.");
                return false;
            }

            // 
            // Set the flags.
            // 

            this.IsLoaded = true;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (this.OnLoaded != null)
            {
                try
                {
                    this.OnLoaded.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ...
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to unload the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryUnloadDriver(IDriver Driver)
        {
            // 
            // Set the flags.
            // 

            this.IsLoaded = false;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (this.OnUnloaded != null)
            {
                try
                {
                    this.OnUnloaded.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ...
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to delete the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public bool TryDeleteDriver(IDriver Driver)
        {
            throw new NotImplementedException("Deleting a manual mapped driver is not supported.");
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