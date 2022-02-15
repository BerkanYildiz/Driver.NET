namespace Driver.NET.DeviceIoControl
{
    using System;
    using System.IO;

    using Microsoft.Win32.SafeHandles;

    public partial class DeviceIoControl : IDeviceIo
    {
        /// <summary>
        /// Gets the safe file handle.
        /// </summary>
        public SafeFileHandle Handle
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path to the symbolic link.
        /// </summary>
        public string SymbolicName
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the handle is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (this.Handle == null)
                    return false;

                return !this.Handle.IsInvalid && !this.Handle.IsClosed;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceIoControl"/> class.
        /// </summary>
        /// <param name="InSymbolicName">The name of the symbolic link.</param>
        public DeviceIoControl(string InSymbolicName)
        {
            this.SymbolicName = InSymbolicName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceIoControl"/> class.
        /// </summary>
        /// <param name="InHandle">A handle to a symbolic link.</param>
        public DeviceIoControl(SafeFileHandle InHandle)
        {
            this.Handle = InHandle;

            if (this.IsValid == false)
            {
                throw new ArgumentException("The handle is invalid.", nameof(InHandle));
            }
        }

        /// <summary>
        /// Opens a handle to the symbolic link file.
        /// </summary>
        public void Connect()
        {
            // 
            // Open a handle to the symbolic file.
            // 

            this.Handle = CreateFile(this.SymbolicName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Closes the file handle previously opened to the symbolic link.
        /// </summary>
        public void Close()
        {
            // 
            // Close the file handle.
            // 

            this.Handle?.Close();
            this.Handle = null;
        }
    }
}