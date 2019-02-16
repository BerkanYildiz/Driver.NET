namespace Driver.Example.Handlers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using Driver.Example.Handlers.Structures;
    using Driver.Logic;
    using Driver.Native;

    using Microsoft.Win32.SafeHandles;

    internal class Requests
    {
        /// <summary>
        /// Gets the driver.
        /// </summary>
        internal Driver Driver
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the lastly used process identifier.
        /// </summary>
        internal int LastProcessId
        {
            get;
            private set;
        }

        private const uint InitCtlCode          = (0x22 << 16) | (0x00 << 14) | (0x700 << 2) | 0x00;
        private const uint ReadCtlCode          = (0x22 << 16) | (0x00 << 14) | (0x701 << 2) | 0x00;
        private const uint WriteCtlCode         = (0x22 << 16) | (0x00 << 14) | (0x702 << 2) | 0x00;
        private const uint VirtualQueryCtlCode  = (0x22 << 16) | (0x00 << 14) | (0x703 << 2) | 0x00;
        private const uint GetBaseAddrCtlCode   = (0x22 << 16) | (0x00 << 14) | (0x704 << 2) | 0x00;
        private const uint UnloadQueryCtlCode   = (0x22 << 16) | (0x00 << 14) | (0x799 << 2) | 0x00;

        /// <summary>
        /// Initializes a new instance of the <see cref="Requests"/> class.
        /// </summary>
        protected Requests()
        {
            // Requests.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Requests"/> class.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        /// <exception cref="ArgumentException">Driver is null.</exception>
        internal Requests(Driver Driver) : this()
        {
            this.SetDriver(Driver);
        }

        /// <summary>
        /// Sets the driver.
        /// </summary>
        /// <param name="Driver">The driver.</param>
        /// <exception cref="System.ArgumentNullException">Driver - Driver is null</exception>
        internal void SetDriver(Driver Driver)
        {
            if (Driver == null)
            {
                throw new ArgumentNullException(nameof(Driver), "Driver is null");
            }

            this.Driver = Driver;
        }

        /// <summary>
        /// Sets the process identifier.
        /// </summary>
        /// <param name="ProcId">The process identifier.</param>
        internal void SetProcId(int ProcId)
        {
            this.LastProcessId = ProcId;
        }

        /// <summary>
        /// Gets the region at the specified address.
        /// </summary>
        internal MemoryBasicInformation? GetMemoryRegion(ulong Address)
        {
            #if DEBUG

            if (!this.Driver.IO.IsConnected)
            {
                throw new Exception("Driver is disconnected.");
            }

            #endif

            if (Address == 0x00)
            {
                return null;
            }

            if (Address >= 0x7FFFFFFEFFFFu)
            {
                return null;
            }

            var Request         = new KernelVirtualQueryRequest
            {
                ProcessId       = this.LastProcessId,
                BaseAddress     = Address,
                Result          = new MemoryBasicInformation(),
                HasBeenFound    = false
            };

            if (!this.Driver.IO.TryIoControl(VirtualQueryCtlCode, Request, out Request))
            {
                return null;
            }

            return Request.Result;
        }

        /// <summary>
        /// Gets the base address of the specified process.
        /// </summary>
        internal ulong GetBaseAddress()
        {
            #if DEBUG

            if (!this.Driver.IO.IsConnected)
            {
                throw new Exception("Driver is disconnected.");
            }

            #endif

            var Request         = new KernelBaseAddrRequest
            {
                ProcessId       = this.LastProcessId,
                BaseAddress     = 0x00,
            };

            if (!this.Driver.IO.TryIoControl(GetBaseAddrCtlCode, Request, out Request))
            {
                return 0x00;
            }

            return Request.BaseAddress;
        }

        /// <summary>
        /// Initializes the unloading of the driver.
        /// </summary>
        internal bool InitUnload()
        {
            #if DEBUG

            if (!this.Driver.IO.IsConnected)
            {
                throw new Exception("Driver is disconnected.");
            }

            #endif

            return this.Driver.IO.TryIoControl(UnloadQueryCtlCode);
        }
    }
}