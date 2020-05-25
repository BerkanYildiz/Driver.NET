namespace Driver.NET.Interfaces
{
    using System;

    public interface IDriverLoader : IDisposable
    {
        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader"/> is loaded.
        /// </summary>
        EventHandler OnLoaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader"/> is unloaded.
        /// </summary>
        EventHandler OnUnloaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverLoader"/> is disposed.
        /// </summary>
        EventHandler OnDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverLoader"/> is loaded.
        /// </summary>
        bool IsLoaded
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverLoader"/> is disposed.
        /// </summary>
        bool IsDisposed
        {
            get;
        }

        /// <summary>
        /// Tries to create the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        bool TryCreateDriver(IDriver Driver);

        /// <summary>
        /// Tries to open the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        bool TryOpenDriver(IDriver Driver);

        /// <summary>
        /// Tries to load the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        bool TryLoadDriver(IDriver Driver);

        /// <summary>
        /// Tries to unload the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        bool TryUnloadDriver(IDriver Driver);

        /// <summary>
        /// Tries to delete the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        bool TryDeleteDriver(IDriver Driver);
    }
}
