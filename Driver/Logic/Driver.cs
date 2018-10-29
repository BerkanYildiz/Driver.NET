namespace Driver.Logic
{
    using System;
    using System.ComponentModel;
    using System.IO;

    using global::Driver.Enums;
    using global::Driver.Logic.Loaders;
    using global::Driver.Logic.Loaders.Interfaces;
    using global::Driver.Utilities;

    using Microsoft.Win32.SafeHandles;

    public class Driver
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
        /// Gets the driver/system file.
        /// </summary>
        public FileInfo DriverFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the symbolic link path.
        /// </summary>
        internal string SymbolicLink
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the safe file handle.
        /// </summary>
        public SafeFileHandle Handle
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the driver loader.
        /// </summary>
        internal IDriverLoad Loader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the driver loading method.
        /// </summary>
        internal DriverLoad LoadMethod
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
        /// <param name="ServiceName">Name of the service.</param>
        /// <param name="SymbolicLink">The sym link.</param>
        /// <param name="DriverFile">The system file.</param>
        /// <param name="LoadType">The driver load type.</param>
        public Driver(string ServiceName, string SymbolicLink, FileInfo DriverFile, DriverLoad LoadType = DriverLoad.Normal) : this()
        {
            this.Setup(ServiceName, SymbolicLink, DriverFile, LoadType);
        }

        /// <summary>
        /// Setups the specified driver.
        /// </summary>
        /// <param name="ServiceName">Name of the service.</param>
        /// <param name="SymbolicLink">The symbolic link.</param>
        /// <param name="DriverFile">The driver file.</param>
        /// <param name="LoadType">Type of the load.</param>
        /// <exception cref="InvalidEnumArgumentException">Invalid DriverLoad method specified.</exception>
        internal void Setup(string ServiceName, string SymbolicLink, FileInfo DriverFile, DriverLoad LoadType = DriverLoad.Normal)
        {
            this.ServiceName    = ServiceName;
            this.SymbolicLink   = SymbolicLink;
            this.DriverFile     = DriverFile;
            this.LoadMethod     = LoadType;

            switch (LoadType)
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

                default:
                {
                    throw new InvalidEnumArgumentException("Invalid DriverLoad method specified.");
                }
            }
        }

        /// <summary>
        /// Sets the loader path.
        /// </summary>
        /// <param name="Path">The path.</param>
        public void SetLoaderPath(string Path)
        {
            switch (this.LoadMethod)
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

            if (!Driver.Exists(this.ServiceName, this.SymbolicLink))
            {
                Log.Info(typeof(Driver), "Driver doesnt exist yet, loading it now at Load().");

                if (this.Loader.LoadDriver())
                {
                    Log.Info(typeof(Driver), "Driver has been successfully mapped and loaded.");
                }
                else
                {
                    Log.Error(typeof(Driver), "Failed to load the driver at Load().");
                    return false;
                }
            }
            else
            {
                Log.Warning(typeof(Driver), "Warning, driver already exist at Load().");
            }

            if (this.Handle != null && (!this.Handle.IsInvalid || !this.Handle.IsClosed))
            {
                Log.Warning(typeof(Driver), "Warning, driver already connected at Load().");

                this.Handle.Close();
                this.Handle = null;
            }

            this.Handle = Native.CreateFile(this.SymbolicLink, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (this.Handle == null || this.Handle.IsInvalid || this.Handle.IsClosed)
            {
                Log.Error(typeof(Driver), "Unable to allocate a mapped file for the driver, aborting.");

                if (!this.Loader.StopDriver())
                {
                    Log.Error(typeof(Driver), "Failed to stop the driver at Load().");
                }

                return false;
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
            if (this.Handle != null && this.Handle.IsInvalid == false)
            {
                this.Handle.Close();
                this.Handle = null;
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
        /// Checks if the specified driver exists.
        /// </summary>
        /// <param name="ServiceName">Name of the service.</param>
        /// <param name="DosName">Name of the dos.</param>
        public static bool Exists(string ServiceName, string DosName)
        {
            var Handle = Native.CreateFile(DosName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            var Exists = (Handle != null && !Handle.IsInvalid);

            if (Handle != null)
            {
                Handle.Close();
            }

            return Exists;
        }

        /// <summary>
        /// Creates a new instance of <see cref="IDriver"/> with the specified arguments,
        /// and loads it according to the specified <see cref="DriverLoad"/> method.
        /// </summary>
        /// <param name="ServiceName">The service name.</param>
        /// <param name="DosName">The dos name.</param>
        /// <param name="File">The system file.</param>
        /// <param name="DriverLoad">The driver load method.</param>
        public static Driver New(string ServiceName, string DosName, FileInfo File, DriverLoad DriverLoad = DriverLoad.Normal)
        {
            if (!Driver.Exists(ServiceName, DosName))
            {
                if (File.Exists == false)
                {
                    throw new FileNotFoundException("The Driver or system file does not exist.");
                }
            }

            var DriverObject = new Driver(ServiceName, DosName, File, DriverLoad);

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
                this.Unload();
            }
            catch (Exception)
            {
                // ...
            }

            if (this.Loader != null)
            {
                this.Loader.Dispose();
            }

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