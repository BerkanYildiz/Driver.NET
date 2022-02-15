namespace Driver.NET.DeviceIoControl
{
    using System;
    using System.Runtime.InteropServices;

    public partial class DeviceIoControl
    {
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
    }
}