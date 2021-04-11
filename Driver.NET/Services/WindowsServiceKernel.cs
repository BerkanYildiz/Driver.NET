namespace Driver.NET.Services
{
    using Driver.NET.Windows;

    public class WindowsServiceKernel
    {
        /// <summary>
        /// Gets the windows service this kernel service is based on.
        /// </summary>
        private WindowsService WindowsService
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsServiceKernel"/> class.
        /// </summary>
        /// <param name="InWindowsService">The windows service this kernel service is based on.</param>
        public WindowsServiceKernel(WindowsService InWindowsService)
        {
            this.WindowsService = InWindowsService;
        }

        /// <summary>
        /// Attempts to start this kernel service on the local machine.
        /// </summary>
        public ulong TryStartDriver()
        {
            // 
            // Acquire the privilege to load kernel drivers.
            // 

            WinApi.AddPrivilege("SeLoadDriverPrivilege");

            // 
            // Manually load the driver by calling the Windows API.
            // 

            var ServiceName = new WindowsService.UNICODE_STRING("\\" + string.Join("\\", "Registry", "Machine", this.WindowsService.RegistryPathRelativeToLocalMachine));
            var ReturnStatus = 0Lu;

            unsafe
            {
                ReturnStatus = WindowsService.NtLoadDriver(&ServiceName);
            }

            // 
            // Release the privilege to load kernel drivers.
            // 

            WinApi.RemovePrivilege("SeLoadDriverPrivilege");
            return ReturnStatus;
        }

        /// <summary>
        /// Attempts to stop this kernel service on the local machine.
        /// </summary>
        public ulong TryStopDriver()
        {
            // 
            // Acquire the privilege to unload kernel drivers.
            // 

            WinApi.AddPrivilege("SeLoadDriverPrivilege");

            // 
            // Manually unload the driver by calling the Windows API.
            // 

            var ServiceName = new WindowsService.UNICODE_STRING("\\" + string.Join("\\", "Registry", "Machine", this.WindowsService.RegistryPathRelativeToLocalMachine));
            var ReturnStatus = 0Lu;

            unsafe
            {
                ReturnStatus = WindowsService.NtUnloadDriver(&ServiceName);
            }

            // 
            // Release the privilege to unload kernel drivers.
            // 

            WinApi.RemovePrivilege("SeLoadDriverPrivilege");
            return ReturnStatus;
        }

        /// <summary>
        /// Converts a Windows Service to a Windows Kernel Driver Service..
        /// </summary>
        /// <param name="Service">The service.</param>
        public static WindowsServiceKernel FromService(WindowsService Service)
        {
            return new WindowsServiceKernel(Service);
        }
    }
}