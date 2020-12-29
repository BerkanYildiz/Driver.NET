namespace Driver.NET
{
    public interface IDeviceIo
    {
        /// <summary>
        /// Gets a value indicating whether the handle is valid.
        /// </summary>
        bool IsValid
        {
            get;
        }

        /// <summary>
        /// Opens a handle to the symbolic link file.
        /// </summary>
        void Connect();

        /// <summary>
        /// Closes the file handle previously opened to the symbolic link.
        /// </summary>
        void Close();

        /// <summary>
        /// Attempts to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        unsafe bool TryIoControl(uint Ioctl, void* InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize);
    }
}
