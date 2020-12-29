namespace Driver.NET.Services
{
    using System.Linq;

    public partial class WindowsService
    {
        /// <summary>
        /// Gets the registry path of this service.
        /// </summary>
        public string RegistryPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the registry path of this service relative to the local machine hive path.
        /// </summary>
        public string RegistryPathRelativeToLocalMachine
        {
            get
            {
                return this.RegistryPath?.Replace("HKEY_LOCAL_MACHINE\\", string.Empty);
            }
        }

        /// <summary>
        /// Gets the name of this service.
        /// </summary>
        public string ServiceName
        {
            get
            {
                return this.RegistryPath?.Split('\\').LastOrDefault();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsService"/> class. 
        /// </summary>
        internal WindowsService()
        {
            // ...
        }
    }
}