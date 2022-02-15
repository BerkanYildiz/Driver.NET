namespace Driver.NET.Example
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    using Driver.NET.DeviceIoControl;
    using Driver.NET.Example.Enums;
    using Driver.NET.Example.Structures;
    using Driver.NET.Services;

    using Microsoft.Win32;

    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="Args">The arguments.</param>
        private static void Main(string[] Args)
        {
            // Example_CreateService();
            Example_QueryDiskSerial();
            Console.ReadKey();
        }

        /// <summary>
        /// Creates and configures a service.
        /// </summary>
        public static void Example_CreateService()
        {
            // 
            // Ensure the registry key exists.
            // 

            var Service = WindowsService.FromServiceName("mydriver"); 
            Service.CreateRegistryKey();

            // 
            // Write values to the service's registry key.
            // 

            Service.WriteRegistryValue("ImagePath", $"System32\\drivers\\mydriver.sys");
            Service.WriteRegistryValue("Type", 1);
            Service.WriteRegistryValue("ErrorControl", 1);
            Service.WriteRegistryValue("Start", 1);
            Service.WriteRegistryValue("Group", "System Reserved");
            Service.WriteRegistryValue("DisplayName", $"@mydriver.inf,%mydriver_ServiceDesc%;Custom Example Driver", RegistryValueKind.ExpandString);
            Service.WriteRegistryValue("Owners", new[] { $"mydriver.inf" });
            Service.WriteRegistryValue("DependOnService", new[] { "myotherdriver" });

            // 
            // Writes values to a sub-key of the service's registry key.
            // 

            Service.WriteRegistryValue("Parameters\\MyCustomParameter", 69);

            // 
            // Start the service as a kernel driver.
            // 

            var KernelService = WindowsServiceKernel.FromService(Service);
            var ReturnStatus = KernelService.TryStartDriver();

            if (ReturnStatus == 0x00000000 ||
                ReturnStatus == 0xC000010E /* DRIVER_ALREADY_STARTED */)
            {
                // 
                // The service has started.
                //   We will now stop it.
                // 

                KernelService.TryStopDriver();
            }

            Console.WriteLine($"[*] CreateService: 0x{ReturnStatus:X8}");

            // 
            // Delete the registry key.
            // 

            Service.DeleteRegistryKey();
        }

        /// <summary>
        /// Gets the serial number of the specified physical driver.
        /// </summary>
        /// <param name="DriveName">Name of the disk drive.</param>
        public static unsafe string Example_QueryDiskSerial(string DriveName = "PhysicalDrive0")
        {
            var SerialNumber = (string) null;

            // 
            // Open the symbolic link created by the disk driver.
            // 

            var DiskDevice = new DeviceIoControl("\\\\.\\" + DriveName);
            DiskDevice.Connect();

            // 
            // Prepare a request to send to the disk driver.
            // 

            var Desc        = new STORAGE_DESCRIPTOR_HEADER();
            var Query       = new STORAGE_PROPERTY_QUERY
            {
                PropertyId  = STORAGE_PROPERTY_ID.StorageDeviceProperty,
                QueryType   = STORAGE_QUERY_TYPE.PropertyStandardQuery
            };

            // 
            // Call the disk driver's IRP handler.
            // 

            if (DiskDevice.TryIoControl(IOCTL_STORAGE_QUERY_PROPERTY, &Query, Marshal.SizeOf(Query), &Desc, Marshal.SizeOf(Desc)))
            {
                var Allocation = Marshal.AllocHGlobal(Desc.Size);

                if (DiskDevice.TryIoControl(IOCTL_STORAGE_QUERY_PROPERTY, &Query, Marshal.SizeOf(Query), Allocation.ToPointer(), Desc.Size))
                {
                    var DeviceDesc = Marshal.PtrToStructure<STORAGE_DEVICE_DESCRIPTOR>(Allocation);

                    if (DeviceDesc.SerialNumberOffset != 0)
                    {
                        SerialNumber = Marshal.PtrToStringAnsi(IntPtr.Add(Allocation, DeviceDesc.SerialNumberOffset), Desc.Size - DeviceDesc.SerialNumberOffset);
                        SerialNumber = SerialNumber.Trim('\0');
                    }
                }
                else
                {
                    Console.WriteLine("[*] Failed to query for the storage descriptor.");
                }

                Marshal.FreeHGlobal(Allocation);
            }
            else
            {
                Console.WriteLine("[*] Failed to query for the storage descriptor size.");
            }

            DiskDevice.Close();

            Console.WriteLine($"[*] Disk Serial: {SerialNumber}");
            return SerialNumber;
        }

        /// <summary>
        /// The IOCTL for storage query property.
        /// </summary>
        private const int IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400;
    }
}
