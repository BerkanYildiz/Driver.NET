namespace Driver.Logic.Loaders
{
    using System;

    using global::Driver.Logic.Loaders.Interfaces;
    using global::Driver.Utilities;

    internal sealed class TurlaLoad : IDriverLoad
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
        /// Creates the specified driver.
        /// </summary>
        public bool CreateDriver(Driver Driver)
        {
            if (Driver == null)
            {
                throw new ArgumentNullException(nameof(Driver), "Driver is null.");
            }

            this.Driver     = Driver;
            this.IsCreated  = true;

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
                return true;
            }

            if (Turla.LoadDriver(this.Driver.DriverFile.FullName))
            {
                this.IsLoaded = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the specified driver.
        /// </summary>
        public bool StopDriver()
        {
            return true;
        }

        /// <summary>
        /// Deletes the specified driver.
        /// </summary>
        public bool DeleteDriver()
        {
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
