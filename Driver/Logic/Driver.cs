namespace Driver.Logic
{
    using System;

    using global::Driver.Logic.Enums;
    using global::Driver.Logic.Interfaces;
    using global::Driver.Logic.Loaders;
    using global::Driver.Utilities;

    public partial class Driver : IDriver
    {
        /// <summary>
        /// Gets or sets the IO requests handler.
        /// </summary>
        public IDriverIo IO
        {
            get;
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
        /// Gets or sets the configuration.
        /// </summary>
        public DriverConfig Config
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

            switch (Config.IoMethod)
            {
                case IoMethod.IoControl:
                {
                    this.IO = new DriverIo(this);
                    break;
                }

                case IoMethod.SharedMemory:
                {
                    this.IO = new DriverIoShared(this);
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid IoMethod specified", nameof(Config.IoMethod));
                }
            }
        }

        /// <summary>
        /// Setups the specified driver.
        /// </summary>
        /// <param name="Config">The driver configuration.</param>
        /// <param name="LoaderPath">The path of the driver loader.</param>
        /// <exception cref="ArgumentException">Invalid LoadType specified.</exception>
        public void Setup(DriverConfig Config, string LoaderPath = null)
        {
            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }

            this.Config = Config;

            if (string.IsNullOrEmpty(Config.ServiceName))
            {
                throw new Exception("Config->ServiceName is null or empty");
            }

            if (string.IsNullOrEmpty(Config.SymbolicLink))
            {
                throw new Exception("Config->SymbolicLink is null or empty");
            }

            if (!string.IsNullOrEmpty(LoaderPath))
            {
                this.SetLoaderPath(LoaderPath);
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
                    break;
                }

                case DriverLoad.Tdl:
                {
                    this.Loader = new TurlaLoad();
                    break;
                }

                case DriverLoad.Capcom:
                {
                    this.Loader = new CapcomLoad();
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid LoadMethod specified", nameof(Config.LoadMethod));
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
                case DriverLoad.Normal:
                {
                    break;
                }

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

                case DriverLoad.Capcom:
                {
                    Capcom.Path = Path;
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid LoadMethod specified", nameof(this.Config.LoadMethod));
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

            if (!Driver.CanConnectTo(this.Config.SymbolicLink, this.Config.IoMethod))
            {
                if (!this.Loader.LoadDriver())
                {
                    Log.Error(typeof(Driver), "Failed to load the driver at Load().");
                    return false;
                }
            }

            this.IsLoaded = true;

            if (this.Config.IoMethod == IoMethod.IoControl)
            {
                if (this.IO.IsConnected)
                {
                    this.IO.Disconnect();
                }
            }

            this.IO.Connect();

            if (!this.IO.IsConnected)
            {
                Log.Error(typeof(Driver), "Failed to initialize the IO communication.");
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
        /// Unloads the currently loaded driver/system file.
        /// </summary>
        public bool Unload()
        {
            if (this.IO.IsConnected)
            {
                this.IO.Disconnect();
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

            this.IO?.Dispose();

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