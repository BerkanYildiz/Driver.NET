namespace ACore.Driver.IO
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    using ACore.Driver.Interfaces;
    using ACore.Native.Structures;
    using ACore.Native.Structures.Enums;

    using Microsoft.Win32.SafeHandles;

    public class DriverIoShared : IDriverIo
    {
        /// <summary>
        /// Gets the shared memory configuration.
        /// </summary>
        public SharedMemoryConfig Config
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the first event waiter.
        /// </summary>
        public AutoResetEvent FirstEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the second event waiter.
        /// </summary>
        public AutoResetEvent SecondEvent
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
        /// Gets a value indicating whether this <see cref="IDriverIo" /> is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the synchronization object lock.
        /// </summary>
        private object Lock
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverIoShared"/> class.
        /// </summary>
        public DriverIoShared()
        {
            this.Lock = new object();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverIoShared"/> class.
        /// </summary>
        /// <param name="Config">The configuration.</param>
        public DriverIoShared(SharedMemoryConfig Config)
        {
            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }

            this.Lock = new object();
            this.SetupSharedMemory(Config);
        }

        /// <summary>
        /// Setups the shared memory system.
        /// </summary>
        /// <param name="Config">The configuration.</param>
        public void SetupSharedMemory(SharedMemoryConfig Config)
        {
            if (Config == null)
            {
                throw new ArgumentNullException(nameof(Config));
            }

            this.Config = Config;
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
            // Setup the shared event system.
            // 

            var FirstEventHandle    = new IntPtr();
            var SecondEventHandle   = new IntPtr();
            var FirstEventAttr      = new OBJECT_ATTRIBUTES(this.Config.FirstEventName, 0x00);
            var SecondEventAttr     = new OBJECT_ATTRIBUTES(this.Config.SecondEventName, 0x00);
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

                Log.Error(this, "Failed to open the first event object. [" + Status + "]");
                return false;
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

                Log.Error(this, "Failed to open the second event object. [" + Status + "]");
                return false;
            }

            FirstEventAlloc.Free();
            SecondEventAlloc.Free();

            // 
            // Turns unmanaged events to managed ones.
            // 

            this.FirstEvent = new AutoResetEvent(false);
            this.FirstEvent.Close();
            this.FirstEvent.SafeWaitHandle = new SafeWaitHandle(FirstEventHandle, false);

            this.SecondEvent = new AutoResetEvent(false);
            this.SecondEvent.Close();
            this.SecondEvent.SafeWaitHandle = new SafeWaitHandle(SecondEventHandle, false);

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
            // Close the events.
            // 

            this.FirstEvent?.Close();
            this.SecondEvent?.Close();

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

            return !this.IsConnected;
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        public bool TryIoControl(uint Ioctl)
        {
            bool Result;

            lock (this.Lock)
            {
                Marshal.WriteInt32(this.Config.MapAddress, (int) Ioctl);

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
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput>(uint Ioctl, TInput InputBuffer, int InputBufferSize)
            where TInput : unmanaged
        {
            bool Result;

            lock (this.Lock)
            {
                var TargetAddr      = this.Config.MapAddress;
                var ContentAddr     = IntPtr.Add(TargetAddr, 0x04);
                
                // ..

                Marshal.WriteInt32(TargetAddr, (int) Ioctl);

                // ..

                var InputSize   = InputBufferSize;
                var InputAddr   = Marshal.AllocHGlobal(InputSize);

                // ..

                Marshal.StructureToPtr<TInput>(InputBuffer, InputAddr, true);

                // ..

                CopyMemory(ContentAddr, InputAddr, (uint) InputSize);

                if (!this.FirstEvent.Set())
                {
                    // ..
                }

                Marshal.FreeHGlobal(InputAddr);

                // ..

                Result = this.SecondEvent.WaitOne();
                this.SecondEvent.Reset();
            }

            return Result;
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, out TOutput OutputBuffer)
            where TOutput : unmanaged
            where TInput : unmanaged
        {
            var InputSize       = Marshal.SizeOf<TInput>();
            var OutputSize      = Marshal.SizeOf<TOutput>();

            return this.TryIoControl(Ioctl, InputBuffer, InputSize, out OutputBuffer, OutputSize);
        }

        /// <summary>
        /// Tries to execute a DeviceIoControl request against the driver.
        /// </summary>
        /// <param name="Ioctl">The IO request control code.</param>
        /// <param name="InputBuffer">The IO request input buffer.</param>
        /// <param name="InputBufferSize">The IO request input buffer size.</param>
        /// <param name="OutputBuffer">The IO request output buffer.</param>
        /// <param name="OutputBufferSize">The IO request output buffer size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryIoControl<TInput, TOutput>(uint Ioctl, TInput InputBuffer, int InputBufferSize, out TOutput OutputBuffer, int OutputBufferSize)
            where TOutput : unmanaged
            where TInput : unmanaged
        {
            bool Result;

            lock (this.Lock)
            {
                var TargetAddr      = this.Config.MapAddress;
                var ContentAddr     = IntPtr.Add(TargetAddr, 0x04);

                // ..

                Marshal.WriteInt32(TargetAddr, (int) Ioctl);

                // ..

                var InputSize       = InputBufferSize;
                var InputAddr       = Marshal.AllocHGlobal(InputSize);

                // ..

                Marshal.StructureToPtr<TInput>(InputBuffer, InputAddr, false);

                // ..

                CopyMemory(ContentAddr, InputAddr, (uint) InputSize);

                if (!this.FirstEvent.Set())
                {
                    // ..
                }

                Marshal.FreeHGlobal(InputAddr);

                // ..

                Result              = this.SecondEvent.WaitOne();
                OutputBuffer        = Marshal.PtrToStructure<TOutput>(ContentAddr);

                // ..

                this.SecondEvent.Reset();
            }

            return Result;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

            // 
            // Make sure we are disconnected first.
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

            this.FirstEvent?.Dispose();
            this.SecondEvent?.Dispose();

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

    public class SharedMemoryConfig
    {
        /// <summary>
        /// Gets or sets the mapped section base address.
        /// </summary>
        public IntPtr MapAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the first event.
        /// </summary>
        public string FirstEventName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the second event.
        /// </summary>
        public string SecondEventName
        {
            get;
            set;
        }
    }
}