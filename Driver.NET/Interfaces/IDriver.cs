namespace Driver.NET.Interfaces
{
    using System;

    public interface IDriver : IDisposable
    {
        /// <summary>
        /// Gets the IO requests handler.
        /// </summary>
        IDriverIo IO
        {
            get;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        DriverConfig Config
        {
            get;
        }

        /// <summary>
        /// Gets or sets the event raised when this driver is loaded.
        /// </summary>
        EventHandler OnLoaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this driver is unloaded.
        /// </summary>
        EventHandler OnUnloaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this driver instance is disposed.
        /// </summary>
        EventHandler OnDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriver"/> is loaded.
        /// </summary>
        bool IsLoaded
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriver"/> is disposed.
        /// </summary>
        bool IsDisposed
        {
            get;
        }

        /// <summary>
        /// Tries to load the driver.
        /// </summary>
        bool TryLoad();

        /// <summary>
        /// Tries to unload the driver.
        /// </summary>
        bool TryUnload();
    }
}
