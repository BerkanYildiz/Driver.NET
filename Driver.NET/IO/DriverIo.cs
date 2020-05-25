namespace Driver.NET.IO
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    using global::Driver.NET.Interfaces;
    using global::Driver.NET.Utils;

    using Microsoft.Win32.SafeHandles;

    public class DriverIo : IDriverIo
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
        public string SymbolicLink
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverIo" /> is connected.
        /// </summary>
        public EventHandler OnConnected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverIo" /> is disconnected.
        /// </summary>
        public EventHandler OnDisconnected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event raised when this <see cref="IDriverIo" /> is disposed.
        /// </summary>
        public EventHandler OnDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverIo" /> is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (this.Handle == null)
                {
                    return false;
                }

                return !this.Handle.IsInvalid && !this.Handle.IsClosed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDriverIo" /> is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverIo"/> class.
        /// </summary>
        public DriverIo()
        {
            // ..
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverIo"/> class.
        /// </summary>
        /// <param name="SymbolicLink">The symbolic link.</param>
        public DriverIo(string SymbolicLink)
        {
            if (string.IsNullOrEmpty(SymbolicLink))
            {
                throw new ArgumentNullException(nameof(SymbolicLink));
            }

            this.SetSymbolicLink(SymbolicLink);
        }

        /// <summary>
        /// Sets the symbolic link.
        /// </summary>
        /// <param name="SymbolicLink">The symbolic link.</param>
        public void SetSymbolicLink(string SymbolicLink)
        {
            if (string.IsNullOrEmpty(SymbolicLink))
            {
                throw new ArgumentNullException(nameof(SymbolicLink));
            }

            this.SymbolicLink = SymbolicLink;
        }

        /// <summary>
        /// Tries to connect with the driver communication system.
        /// </summary>
        public bool TryConnect()
        {
            if (this.IsConnected)
            {
                return true;
            }

            // 
            // Open a handle to the symbolic file.
            // 

            this.Handle = WinApi.CreateFile(this.SymbolicLink, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (this.IsConnected)
            {
                if (this.OnConnected != null)
                {
                    try
                    {
                        this.OnConnected.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }

            return this.IsConnected;
        }

        /// <summary>
        /// Tries to disconnect from the driver communication system.
        /// </summary>
        public bool TryDisconnect()
        {
            if (!this.IsConnected)
            {
                return true;
            }

            // 
            // Close the file handle.
            // 

            this.Handle.Close();
            this.Handle = null;

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (!this.IsConnected)
            {
                if (this.OnDisconnected != null)
                {
                    try
                    {
                        this.OnDisconnected.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        public unsafe bool TryIoControl(uint Ioctl)
        {
            return NtDeviceIoControl(this.Handle, Ioctl, null, 0, null, 0, out var ReturnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        public bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer)
            where TInput : unmanaged
        {
            return this.TryIoControl(Ioctl, InputBuffer, Marshal.SizeOf<TInput>());
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        public unsafe bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize)
            where TInput : unmanaged
        {
            return NtDeviceIoControl(this.Handle, Ioctl, &InputBuffer, InputBufferSize, null, 0, out var ReturnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        public bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, out TOutput OutputBuffer)
            where TOutput : unmanaged
            where TInput : unmanaged
        {
            return this.TryIoControl(Ioctl, InputBuffer, Marshal.SizeOf<TInput>(), out OutputBuffer, Marshal.SizeOf<TOutput>());
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        public unsafe bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, out TOutput OutputBuffer, int OutputBufferSize)
            where TOutput : unmanaged
            where TInput : unmanaged 
        {
            OutputBuffer = new TOutput();

            fixed (void* RealOutputBuffer = &OutputBuffer)
            {
                return NtDeviceIoControl(this.Handle, Ioctl, &InputBuffer, InputBufferSize, RealOutputBuffer, OutputBufferSize, out var ReturnedBytes, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        public unsafe bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize)
            where TInput : unmanaged 
        {
            return NtDeviceIoControl(this.Handle, Ioctl, &InputBuffer, InputBufferSize, OutputBuffer, OutputBufferSize, out var ReturnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        public unsafe bool TryIoControl(uint Ioctl, void* InputBuffer, int InputBufferSize, void* OutputBuffer, int OutputBufferSize)
        {
            return NtDeviceIoControl(this.Handle, Ioctl, InputBuffer, InputBufferSize, OutputBuffer, OutputBufferSize, out var ReturnedBytes, IntPtr.Zero);
        }

        /// <summary>
        /// Performs application-defined tasks associated with
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            // 
            // Make sure we are disconnected before disposing.
            // 

            if (this.IsConnected)
            {
                try
                {
                    this.TryDisconnect();
                }
                catch (Exception)
                {
                    // ..
                }
            }

            // 
            // Dispose the managed instances.
            // 

            this.Handle?.Dispose();

            // 
            // Execute the handlers subscribed to the event.
            // 

            if (this.OnDisposed != null)
            {
                try
                {
                    this.OnDisposed.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ...
                }
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern unsafe bool NtDeviceIoControl(
            SafeFileHandle Handle,
            uint IoControlCode,
            void* InputBuffer,
            int InputBufferSize,
            void* OutBuffer,
            int OutputBufferSize,
            out int ReturnedBytes,
            IntPtr Overlapped
        );
    }
}