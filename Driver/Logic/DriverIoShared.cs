namespace Driver.Logic
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    using global::Driver.Logic.Interfaces;
    using global::Driver.Native;
    using global::Driver.Native.Enums;

    using Microsoft.Win32.SafeHandles;

    public class DriverIoShared : IDriverIo
    {
        /// <summary>
        /// Gets or sets the <see cref="IDriver" />.
        /// </summary>
        public IDriver Driver
        {
            get;
        }

        /// <summary>
        /// Gets or sets the safe file handle.
        /// </summary>
        public SafeFileHandle Handle
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the mapped section base address.
        /// </summary>
        public IntPtr MapAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the first event waiter.
        /// </summary>
        public AutoResetEvent FirstEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the second event waiter.
        /// </summary>
        public AutoResetEvent SecondEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the connected event.
        /// </summary>
        public EventHandler Connected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the disconnected event.
        /// </summary>
        public EventHandler Disconnected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the disposed event.
        /// </summary>
        public EventHandler Disposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the synchronization object lock.
        /// </summary>
        public object Lock
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DriverIo"/> is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (this.FirstEvent != null)
                {
                    if (!this.FirstEvent.SafeWaitHandle.IsInvalid && !this.FirstEvent.SafeWaitHandle.IsClosed)
                    {
                        return true;
                    }
                }

                if (this.SecondEvent != null)
                {
                    if (!this.SecondEvent.SafeWaitHandle.IsInvalid && !this.SecondEvent.SafeWaitHandle.IsClosed)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DriverIoShared"/> is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DriverIoShared"/> class from being created.
        /// </summary>
        protected DriverIoShared()
        {
            // DriverIoShared.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverIoShared"/> class.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        public DriverIoShared(IDriver Driver)
        {
            this.Lock = new object();
            this.Driver = Driver;

            if (this.Driver.Config.SharedMemory.ProcessAddr > 0)
            {
                return;
            }

            this.MapAddress = Marshal.AllocHGlobal(0x1000);

            if (this.MapAddress == IntPtr.Zero)
            {
                throw new InsufficientMemoryException();
            }

            this.Driver.Config.SharedMemory.ProcessAddr = (ulong) this.MapAddress.ToInt64();
        }

        /// <summary>
        /// Connects this instance to the driver.
        /// </summary>
        /// <exception cref="Exception">The driver has to be loaded before connecting</exception>
        public void Connect()
        {
            if (this.IsConnected)
            {
                return;
            }

            var FirstEventHandle    = new IntPtr();
            var SecondEventHandle   = new IntPtr();
            var FirstEventAttr      = new OBJECT_ATTRIBUTES(this.Driver.Config.SharedMemory.FirstEventName, 0x00);
            var SecondEventAttr     = new OBJECT_ATTRIBUTES(this.Driver.Config.SharedMemory.SecondEventName, 0x00);
            var FirstEventAlloc     = GCHandle.Alloc(FirstEventAttr, GCHandleType.Pinned);
            var SecondEventAlloc    = GCHandle.Alloc(SecondEventAttr, GCHandleType.Pinned);

            var Status              = (NtStatus) NtOpenEvent(ref FirstEventHandle, (uint) AccessMask.EVENT_ALL_ACCESS, FirstEventAlloc.AddrOfPinnedObject());

            if (Status > 0x00)
            {
                FirstEventAlloc.Free();
                SecondEventAlloc.Free();

                if (FirstEventHandle != IntPtr.Zero)
                {
                    NtClose(FirstEventHandle);
                }

                if (SecondEventHandle != IntPtr.Zero)
                {
                    NtClose(SecondEventHandle);
                }

                Log.Error(typeof(DriverIoShared), "Failed to open the first event object. [" + Status + "]");
                return;
            }

            Status                  = (NtStatus) NtOpenEvent(ref SecondEventHandle, (uint) AccessMask.EVENT_ALL_ACCESS, SecondEventAlloc.AddrOfPinnedObject());

            if (Status > 0x00)
            {
                FirstEventAlloc.Free();
                SecondEventAlloc.Free();

                if (FirstEventHandle != IntPtr.Zero)
                {
                    NtClose(FirstEventHandle);
                }

                if (SecondEventHandle != IntPtr.Zero)
                {
                    NtClose(SecondEventHandle);
                }

                Log.Error(typeof(DriverIoShared), "Failed to open the second event object. [" + Status + "]");
                return;
            }

            FirstEventAlloc.Free();
            SecondEventAlloc.Free();

            // ..

            this.FirstEvent = new AutoResetEvent(false);
            this.FirstEvent.Close();
            this.FirstEvent.SafeWaitHandle = new SafeWaitHandle(FirstEventHandle, false);

            this.SecondEvent = new AutoResetEvent(false);
            this.SecondEvent.Close();
            this.SecondEvent.SafeWaitHandle = new SafeWaitHandle(SecondEventHandle, false);

            if (this.IsConnected && this.Connected != null)
            {
                try
                {
                    this.Connected.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ..
                }
            }
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="IoCtl">The IO request control code.</param>
        public bool TryIoControl(uint IoCtl)
        {
            bool Result;

            lock (this.Lock)
            {
                Marshal.WriteInt32(this.MapAddress, (int)IoCtl);

                if (!this.FirstEvent.Set())
                {
                    // ..
                }

                Result = this.SecondEvent.WaitOne();
                this.SecondEvent.Reset();
            }

            return Result;
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="IoCtl">The IO request control code.</param>
        /// <param name="IoData">The IO request data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput>(uint IoCtl, TInput IoData)
        {
            return this.TryIoControl(IoCtl, IoData, Marshal.SizeOf<TInput>());
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="IoCtl">The IO request control code.</param>
        /// <param name="IoData">The IO request data.</param>
        /// <param name="IoDataSize">The IO request data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput>(uint IoCtl, TInput IoData, int IoDataSize)
        {
            bool Result;

            lock (this.Lock)
            {
                var TargetAddr      = this.MapAddress;
                var ContentAddr     = IntPtr.Add(TargetAddr, 0x04);

                Marshal.WriteInt32(TargetAddr, (int) IoCtl);

                if (IoData != null)
                {
                    var InputSize   = IoDataSize;
                    var InputAddr   = Marshal.AllocHGlobal(InputSize);

                    Marshal.StructureToPtr<TInput>(IoData, InputAddr, true);

                    CopyMemory(ContentAddr, InputAddr, (uint) InputSize);

                    if (!this.FirstEvent.Set())
                    {
                        // ..
                    }

                    Marshal.FreeHGlobal(InputAddr);
                }

                Result = this.SecondEvent.WaitOne();
                this.SecondEvent.Reset();
            }

            return Result;
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="IoCtl">The IO request control code.</param>
        /// <param name="IoData">The IO request data.</param>
        /// <param name="IoOutput">The IO request output data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput, TOutput>(uint IoCtl, TInput IoData, out TOutput IoOutput)
        {
            var InputSize       = Marshal.SizeOf<TInput>();
            var OutputSize      = Marshal.SizeOf<TOutput>();

            return this.TryIoControl(IoCtl, IoData, InputSize, out IoOutput, OutputSize);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="IoCtl">The IO request control code.</param>
        /// <param name="IoData">The IO request data.</param>
        /// <param name="IoDataSize">The IO request data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput, TOutput>(uint IoCtl, TInput IoData, int IoDataSize, out TOutput IoOutput, int IoOutputSize)
        {
            bool Result;

            lock (this.Lock)
            {
                var TargetAddr      = this.MapAddress;
                var ContentAddr     = IntPtr.Add(TargetAddr, 0x04);

                Marshal.WriteInt32(TargetAddr, (int) IoCtl);

                if (IoData != null)
                {
                    var InputSize   = IoDataSize;
                    var InputAddr   = Marshal.AllocHGlobal(InputSize);

                    Marshal.StructureToPtr<TInput>(IoData, InputAddr, true);

                    CopyMemory(ContentAddr, InputAddr, (uint) InputSize);

                    if (!this.FirstEvent.Set())
                    {
                        // ..
                    }

                    Marshal.FreeHGlobal(InputAddr);
                }

                Result              = this.SecondEvent.WaitOne();
                IoOutput            = Marshal.PtrToStructure<TOutput>(ContentAddr);

                this.SecondEvent.Reset();
            }

            return Result;
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="IoCtl">The IO request control code.</param>
        /// <param name="IoData">The IO request data.</param>
        /// <param name="IoDataSize">The IO request data size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput, TOutput>(uint IoCtl, TInput IoData, int IoDataSize, out TOutput IoOutput, int IoOutputSize, ref int IoDataReceived)
        {
            return this.TryIoControl(IoCtl, IoData, IoDataSize, out IoOutput, IoOutputSize);
        }

        /// <summary>
        /// Disconnects this instance from the driver.
        /// </summary>
        /// <exception cref="Exception">The driver has to be loaded before disconnecting</exception>
        public void Disconnect()
        {
            if (this.IsConnected)
            {
                this.Handle?.Close();
                this.FirstEvent?.Close();
                this.SecondEvent?.Close();

                if (this.Disconnected != null)
                {
                    try
                    {
                        this.Disconnected.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        // ..
                    }
                }
            }
        }

        /// <summary>
        /// Exécute les tâches définies par l'application associées
        /// à la libération ou à la redéfinition des ressources non managées.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            // ...

            this.Disconnect();

            // ..

            this.Handle?.Dispose();
            this.FirstEvent?.Dispose();
            this.SecondEvent?.Dispose();

            // ..

            this.Handle = null;
            this.FirstEvent = null;
            this.SecondEvent = null;

            // ..

            if (this.MapAddress != IntPtr.Zero)
            {
                try
                {
                    Marshal.FreeHGlobal(this.MapAddress);
                }
                catch (Exception)
                {
                    // ..
                }
            }

            // ..

            if (this.Disposed != null)
            {
                try
                {
                    this.Disposed.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                    // ...
                }
            }
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtOpenEvent(
            ref IntPtr EventHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtClose(
            IntPtr Handle);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(
            IntPtr Destination,
            IntPtr Source,
            uint Length);
    }
}