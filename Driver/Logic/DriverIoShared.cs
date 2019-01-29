namespace Driver.Logic
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    using global::Driver.Logic.Interfaces;
    using global::Driver.Native;
    using global::Driver.Native.Enums;
    using global::Driver.Native.Enums.Memory;

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
        /// Gets or sets the event safe file handle.
        /// </summary>
        public SafeFileHandle EventHandle
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
        /// Gets or sets the waiter.
        /// </summary>
        public AutoResetEvent Waiter
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
        /// Gets a value indicating whether this <see cref="DriverIoShared"/> is initialized.
        /// </summary>
        public bool IsInitialized
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
                if (this.Handle != null)
                {
                    if (!this.Handle.IsInvalid && !this.Handle.IsClosed)
                    {
                        return true;
                    }
                }

                if (this.EventHandle != null)
                {
                    if (!this.EventHandle.IsInvalid && !this.EventHandle.IsClosed)
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
            this.Driver         = Driver;
            this.IsInitialized  = this.TryInitialize();

            if (!this.IsInitialized)
            {
                Log.Error(typeof(DriverIoShared), "Failed to initialize the DriverIoShared instance.");
            }
        }

        /// <summary>
        /// Tries to initialize this instance.
        /// </summary>
        public bool TryInitialize()
        {
            var Status          = NtStatus.Success;
            var MaximumSize     = 0x1000Lu;
            var OwnsSection     = true;
            var OwnsEvent       = true;
            var EventHandle     = new IntPtr();
            var SectionHandle   = new IntPtr();
            var BaseAddress     = new IntPtr();
            var Attributes      = new OBJECT_ATTRIBUTES(@"\BaseNamedObjects\" + this.Driver.Config.ServiceName, 0x00);
            var AttrAlloc       = GCHandle.Alloc(Attributes, GCHandleType.Pinned);

            Status              = (NtStatus) NtCreateSection(ref SectionHandle, (uint) AccessMask.SECTION_ALL_ACCESS, AttrAlloc.AddrOfPinnedObject(), ref MaximumSize, (uint) MemoryPagePermissions.PAGE_READWRITE, (uint) 0x8000000, IntPtr.Zero);

            if (Status > 0x00)
            {
                if (Status == NtStatus.ObjectNameCollision)
                {
                    OwnsSection = false;
                    Status      = (NtStatus) NtOpenSection(ref SectionHandle, (uint) AccessMask.SECTION_ALL_ACCESS, AttrAlloc.AddrOfPinnedObject());
                }

                if (Status > 0x00)
                {
                    AttrAlloc.Free();
                    Log.Error(typeof(DriverIoShared), "Failed to create section object. [" + Status + "]");
                    return false;
                }
            }

            AttrAlloc.Free();

            Attributes          = new OBJECT_ATTRIBUTES(@"\BaseNamedObjects\" + this.Driver.Config.ServiceName + "Event", 0x00);
            AttrAlloc           = GCHandle.Alloc(Attributes, GCHandleType.Pinned);

            Status              = (NtStatus) NtCreateEvent(ref EventHandle, (uint) AccessMask.EVENT_ALL_ACCESS, AttrAlloc.AddrOfPinnedObject(), (uint) EventType.SynchronizationEvent, false);

            if (Status > 0x00)
            {
                if (Status == NtStatus.ObjectNameCollision)
                {
                    OwnsEvent   = false;
                    Status      = (NtStatus) NtOpenEvent(ref EventHandle, (uint) AccessMask.EVENT_ALL_ACCESS, AttrAlloc.AddrOfPinnedObject());
                }

                if (Status > 0x00)
                {
                    AttrAlloc.Free();
                    Log.Error(typeof(DriverIoShared), "Failed to create the event object. [" + Status + "]");
                    return false;
                }
            }

            AttrAlloc.Free();

            Status              = (NtStatus) NtMapViewOfSection(SectionHandle, Process.GetCurrentProcess().Handle, ref BaseAddress, UIntPtr.Zero, (UIntPtr) MaximumSize, out var SectionOffset, out var ViewSize, (uint) SectionInherit.ViewUnmap, 0, (uint) MemoryPagePermissions.PAGE_READWRITE);

            if (Status > 0x00)
            {
                Log.Error(typeof(DriverIoShared), "Failed to map the section object. [" + Status + "]");
                return false;
            }

            this.Waiter = new AutoResetEvent(false);
            this.Waiter.Close();
            this.Waiter.SafeWaitHandle = new SafeWaitHandle(EventHandle, true);

            this.Handle         = new SafeFileHandle(SectionHandle, OwnsSection);
            this.EventHandle    = new SafeFileHandle(EventHandle, OwnsEvent);
            this.MapAddress     = BaseAddress;

            return true;
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
            return false;
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
            var TargetAddr      = this.MapAddress;
            var ContentAddr     = IntPtr.Add(TargetAddr, 0x04);

            Marshal.WriteInt32(TargetAddr, (int) IoCtl);

            if (IoData != null)
            {
                var InputSize   = IoDataSize;
                var InputAddr   = Marshal.AllocHGlobal(InputSize);

                Marshal.StructureToPtr<TInput>(IoData, InputAddr, true);

                CopyMemory(ContentAddr, InputAddr, (uint) InputSize);

                if (!this.Waiter.Set())
                {
                    // ..
                }

                Marshal.FreeHGlobal(InputAddr);
            }

            return this.Waiter.WaitOne();
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
            IoOutput            = default(TOutput);

            var TargetAddr      = this.MapAddress;
            var ContentAddr     = IntPtr.Add(TargetAddr, 0x04);

            Marshal.WriteInt32(TargetAddr, (int) IoCtl);

            if (IoData != null)
            {
                var InputSize   = IoDataSize;
                var InputAddr   = Marshal.AllocHGlobal(InputSize);

                Marshal.StructureToPtr<TInput>(IoData, InputAddr, true);

                CopyMemory(ContentAddr, InputAddr, (uint) InputSize);

                if (!this.Waiter.Set())
                {
                    // ..
                }

                Marshal.FreeHGlobal(InputAddr);
            }

            var IsSuccessful    = this.Waiter.WaitOne();

            if (IsSuccessful)
            {
                IoOutput        = Marshal.PtrToStructure<TOutput>(ContentAddr);
            }

            return IsSuccessful;
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
                this.EventHandle?.Close();
                this.Waiter?.Close();

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
            this.EventHandle?.Dispose();
            this.Waiter?.Dispose();

            // ..

            this.Handle = null;
            this.EventHandle = null;
            this.Waiter = null;

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
        private static extern uint NtCreateSection(
            ref IntPtr SectionHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes,
            ref ulong MaximumSize,
            uint SectionPageProtection,
            uint AllocationAttributes,
            IntPtr FileHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtOpenSection(
            ref IntPtr SectionHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtMapViewOfSection(
            IntPtr SectionHandle,
            IntPtr ProcessHandle,
            ref IntPtr BaseAddress,
            UIntPtr ZeroBits,
            UIntPtr CommitSize,
            out ulong SectionOffset,
            out uint ViewSize,
            uint InheritDisposition,
            uint AllocationType,
            uint Win32Protect);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtCreateEvent(
            ref IntPtr EventHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes,
            uint EventType,
            bool InitialState);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtOpenEvent(
            ref IntPtr EventHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(
            IntPtr Destination,
            IntPtr Source,
            uint Length);
    }
}