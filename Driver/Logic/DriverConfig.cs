namespace Driver.Logic
{
    using System.IO;

    using global::Driver.Logic.Enums;

    public class DriverConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriverConfig"/> class.
        /// </summary>
        public DriverConfig()
        {
            this.SharedMemory = new DriverConfigSharedMemory();
        }

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

        /// <summary>
        /// Gets the shared memory configuration.
        /// </summary>
        public DriverConfigSharedMemory SharedMemory
        {
            get;
            set;
        }

        /// <summary>
        /// The shared memory configuration class.
        /// </summary>
        public class DriverConfigSharedMemory
        {
            /// <summary>
            /// Gets or sets the process identifier.
            /// </summary>
            public int ProcessId
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the process address.
            /// </summary>
            public ulong ProcessAddr
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the first event name.
            /// </summary>
            public string FirstEventName
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the second event name.
            /// </summary>
            public string SecondEventName
            {
                get;
                set;
            }
        }
    }
}
