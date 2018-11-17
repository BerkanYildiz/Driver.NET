namespace Driver.Logic
{
    using System;
    using System.IO;

    using global::Driver.Enums;
    using global::Driver.Logic.Loaders;
    using global::Driver.Logic.Loaders.Interfaces;
    using global::Driver.Utilities;

    using Microsoft.Win32.SafeHandles;

    public partial class Driver : IDisposable
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        internal DriverConfig Config
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the driver loader.
        /// </summary>
        internal IDriverLoad Loader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the safe file handle.
        /// </summary>
        public SafeFileHandle Handle
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the load event.
        /// </summary>
        public EventHandler Loaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connected event.
        /// </summary>
        public EventHandler Connected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unload event.
        /// </summary>
        public EventHandler Unloaded
        {
            get;
            set;
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
        /// Gets a value indicating whether this <see cref="Driver"/> is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Driver"/> is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (this.Handle == null)
                {
                    return false;
                }

                return !this.Handle.IsInvalid && !this.Handle.IsClosed;
            }
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
        /// Prevents a default instance of the <see cref="Driver"/> class from being created.
        /// </summary>
        protected Driver()
        {
            // Driver.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Driver"/> class.
        /// </summary>
        /// <param name="Config">The configuration.</param>
        /// <param name="LoaderPath">The path of the driver loader.</param>
        public Driver(DriverConfig Config, string LoaderPath = null)
        {
            this.Setup(Config, LoaderPath);
        }

        /// <summary>
        /// Setups the specified driver.
        /// </summary>
        /// <param name="Config">The driver configuration.</param>
        /// <param name="LoaderPath">The path of the driver loader.</param>
        /// <exception cref="ArgumentException">Invalid LoadType specified.</exception>
        public void Setup(DriverConfig Config, string LoaderPath = null)
        {
            this.Config = Config;

            if (string.IsNullOrEmpty(Config.ServiceName))
            {
                throw new Exception("Config->ServiceName is null or empty");
            }

            if (string.IsNullOrEmpty(Config.SymbolicLink))
            {
                throw new Exception("Config->SymbolicLink is null or empty");
            }

            switch (this.Config.LoadMethod)
            {
                case DriverLoad.Normal:
                {
                    this.Loader = new ServiceLoad();
                    break;
                }

                case DriverLoad.Dse:
                {
                    this.Loader = new DseLoad();

                    if (!string.IsNullOrEmpty(LoaderPath))
                    {
                        this.SetLoaderPath(LoaderPath);
                    }

                    break;
                }

                case DriverLoad.Tdl:
                {
                    this.Loader = new TurlaLoad();

                    if (!string.IsNullOrEmpty(LoaderPath))
                    {
                        this.SetLoaderPath(LoaderPath);
                    }

                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid LoadType specified", nameof(Config.LoadMethod));
                }
            }
        }

        /// <summary>
        /// Sets the loader path.
        /// </summary>
        /// <param name="Path">The path.</param>
        public void SetLoaderPath(string Path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException(nameof(Path));
            }

            switch (this.Config.LoadMethod)
            {
                case DriverLoad.Dse:
                {
                    DSEFix.Path = Path;
                    break;
                }

                case DriverLoad.Tdl:
                {
                    Turla.Path = Path;
                    break;
                }

                default:
                {
                    throw new InvalidOperationException("Unable to set the loader path if the load type is neither Dse or Tdl");
                }
            }
        }

        /// <summary>
        /// Loads the specified driver/system file.
        /// </summary>
        public bool Load()
        {
            if (!this.Loader.CreateDriver(this))
            {
                Log.Error(typeof(Driver), "Failed to create the driver at Load().");
                return false;
            }

            if (!Driver.CanConnectTo(this.Config.SymbolicLink))
            {
                if (!this.Loader.LoadDriver())
                {
                    Log.Error(typeof(Driver), "Failed to load the driver at Load().");
                    return false;
                }
            }
            else
            {
                Log.Warning(typeof(Driver), "Warning, driver already exist at Load().");
            }

            this.IsLoaded = true;

            if (this.IsConnected)
            {
                this.Disconnect();
            }

            this.Connect();

            if (!this.IsConnected)
            {
                Log.Error(typeof(Driver), "Failed to open the symbolic file.");
            }

            if (this.Loaded != null)
            {
                try
                {
                    this.Loaded.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ..
                }
            }

            return true;
        }

        /// <summary>
        /// Connects this instance to the driver.
        /// </summary>
        /// <exception cref="Exception">The driver has to be loaded before connecting</exception>
        public void Connect()
        {
            if (this.IsConnected)
            {
                this.Disconnect();
            }

            this.Handle = Native.CreateFile(this.Config.SymbolicLink, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (this.IsConnected && this.Connected != null)
            {
                try
                {
                    this.Connected.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ..
                }
            }
        }

        /// <summary>
        /// Disconnects this instance from the driver.
        /// </summary>
        /// <exception cref="Exception">The driver has to be loaded before disconnecting</exception>
        public void Disconnect()
        {
            if (this.IsConnected)
            {
                this.Handle.Close();
            }
            else
            {
                this.Handle = null;
            }
        }

        /// <summary>
        /// Unloads the currently loaded driver/system file.
        /// </summary>
        public bool Unload()
        {
            if (this.IsConnected)
            {
                this.Disconnect();
            }

            if (!this.Loader.StopDriver())
            {
                Log.Error(typeof(Driver), "Failed to unload the driver at Unload().");
                return false;
            }

            if (!this.Loader.DeleteDriver())
            {
                Log.Error(typeof(Driver), "Failed to delete the driver at Unload().");
                return false;
            }

            this.IsLoaded = false;

            if (this.Unloaded != null)
            {
                try
                {
                    this.Unloaded.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ..
                }
            }

            return true;
        }

        /// <summary>
        /// Exécute les tâches définies par l'application associées
        /// à la libération ou à la redéfinition des ressources non managées.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            // ...

            try
            {
                if (!this.Unload())
                {
                    // R.I.P
                }
            }
            catch (Exception)
            {
                // VERY R.I.P
            }

            // ..

            if (this.Disposed != null)
            {
                try
                {
                    this.Disposed.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ...
                }
            }
        }
    }
}