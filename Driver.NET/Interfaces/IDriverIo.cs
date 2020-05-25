namespace Driver.NET.Interfaces
{
    using System;

    public interface IDriverIo : IDisposable
    {
        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverIo"/> is connected.
        /// </summary>
        EventHandler OnConnected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverIo"/> is disconnected.
        /// </summary>
        EventHandler OnDisconnected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverIo"/> is disposed.
        /// </summary>
        EventHandler OnDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverIo"/> is connected.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverIo"/> is disposed.
        /// </summary>
        bool IsDisposed
        {
            get;
        }

        /// <summary>
        /// Tries to connect with the driver communication system.
        /// </summary>
        bool TryConnect();

        /// <summary>
        /// Tries to disconnect from the driver communication system.
        /// </summary>
        bool TryDisconnect();

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        bool TryIoControl(uint Ioctl);

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer)
            where TInput : unmanaged;

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize)
            where TInput : unmanaged;

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, out TOutput OutputBuffer)
            where TOutput : unmanaged
            where TInput : unmanaged;

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, out TOutput OutputBuffer, int OutputBufferSize)
            where TOutput : unmanaged
            where TInput : unmanaged;

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        unsafe bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize)
            where TInput : unmanaged;

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        unsafe bool TryIoControl(uint Ioctl, void* InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize);
    }
}
