namespace Driver.Logic
{
    using System.IO;

    using global::Driver.Logic.Enums;

    public class DriverConfig
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string ServiceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the driver/system file.
        /// </summary>
        public FileInfo DriverFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the symbolic link path.
        /// </summary>
        public string SymbolicLink
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the driver loading method.
        /// </summary>
        public DriverLoad LoadMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the IO communication method.
        /// </summary>
        public IoMethod IoMethod
        {
            get;
            set;
        } = IoMethod.IoControl;
    }
}
