namespace Driver.Logic.Loaders
{
    using System;

    using global::Driver.Logic.Enums;
    using global::Driver.Logic.Interfaces;
    using global::Driver.Utilities;

    internal sealed class IntelMapperLoad : IDriverLoad
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
        /// Creates the specified driver.
        /// </summary>
        public bool CreateDriver(Driver Driver)
        {
            var Config = Driver.Config;

            if (this.IsCreated)
            {
                throw new Exception("Driver is already created");
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

            var SharedMemory = this.Driver.Config.SharedMemory;

            if (this.IsLoaded)
            {
                return true;
            }

            if (this.Driver.Config.IoMethod == IoMethod.SharedMemory && SharedMemory != null)
            {
                this.IsLoaded = IntelMapper.LoadDriver(this.Driver.Config.DriverFile?.FullName, SharedMemory.ProcessId, SharedMemory.ProcessAddr, SharedMemory.FirstEventName, SharedMemory.SecondEventName);
            }
            else
            {
                this.IsLoaded = IntelMapper.LoadDriver(this.Driver.Config.DriverFile?.FullName);
            }

            if (this.IsLoaded)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the specified driver.
        /// </summary>
        public bool StopDriver()
        {
            if (!this.IsCreated)
            {
                throw new Exception("Driver is not created.");
            }

            if (!this.IsLoaded)
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// Deletes the specified driver.
        /// </summary>
        public bool DeleteDriver()
        {
            if (!this.IsCreated)
            {
                throw new Exception("Driver is not created.");
            }

            if (this.IsLoaded)
            {
                if (!this.StopDriver())
                {
                    return false;
                }
            }

            this.IsCreated  = false;

            return true;
        }
    }
}
