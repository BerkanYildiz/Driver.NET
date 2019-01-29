namespace Driver.Logic.Interfaces
{
    using System;

    public interface IDriver : IDisposable
    {
        /// <summary>
        /// Gets or sets the IO requests handler.
        /// </summary>
        IDriverIo IO
        {
            get;
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        DriverConfig Config
        {
            get;
        }

        /// <summary>
        /// Gets or sets the load event.
        /// </summary>
        EventHandler Loaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unload event.
        /// </summary>
        EventHandler Unloaded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the disposed event.
        /// </summary>
        EventHandler Disposed
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
        /// Loads the specified driver/system file.
        /// </summary>
        bool Load();

        /// <summary>
        /// Unloads the currently loaded driver/system file.
        /// </summary>
        bool Unload();
    }
}
