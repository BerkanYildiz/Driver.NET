namespace Driver.NET.Example
{
    using System;
    using System.Runtime.InteropServices;

    using global::Driver.NET.Example.Enums;
    using global::Driver.NET.Example.Structures;
    using global::Driver.NET.IO;
    using global::Driver.NET.Loaders;

    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="Args">The arguments.</param>
        private static void Main(string[] Args)
        {
            for (var I = 0; I < 10; I++)
            {
                var DiskSerialNumber = GetSerialNumberOf("PhysicalDrive" + I);

                if (DiskSerialNumber == null)
                {
                    continue;
                }

                Console.WriteLine($"[*] PhysicalDrive{I}: ");
                Console.WriteLine($"[*]    SerialNumber: {DiskSerialNumber}");
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Gets the serial number of the specified physical driver.
        /// </summary>
        /// <param name="DriveName">Name of the disk drive.</param>
        public static unsafe string GetSerialNumberOf(string DriveName = "PhysicalDrive0")
        {
            var SerialNumber = (string) null;

            // 
            // Setup the driver's handlers/settings.
            // 

            var DriverLoader = new KernelServiceLoader();
            DriverLoader.SetServiceName("disk");
            DriverLoader.SetOwnership(false);

            var DriverIo = new DriverIo();
            DriverIo.SetSymbolicLink("\\\\.\\" + DriveName);

            // 
            // Initialize the driver interface.
            // 

            using (var Driver = new Driver(new DriverConfig(), DriverLoader, DriverIo))
            {
                // 
                // Opens or starts the disk driver.
                // 

                if (!Driver.TryLoad())
                {
                    Console.WriteLine("[*] Failed to load the driver.");
                    goto End;
                }

                // 
                // Connects with the disk driver.
                // 

                if (Driver.IO.TryConnect())
                {
                    var Desc        = new STORAGE_DESCRIPTOR_HEADER();
                    var Query       = new STORAGE_PROPERTY_QUERY
                    {
                        PropertyId  = STORAGE_PROPERTY_ID.StorageDeviceProperty,
                        QueryType   = STORAGE_QUERY_TYPE.PropertyStandardQuery
                    };

                    // 
                    // Call the disk driver's IRP handler.
                    // 

                    if (Driver.IO.TryIoControl(IOCTL_STORAGE_QUERY_PROPERTY, &Query, Marshal.SizeOf(Query), &Desc, Marshal.SizeOf(Desc)))
                    {
                        var Allocation = Marshal.AllocHGlobal(Desc.Size);

                        if (Driver.IO.TryIoControl(IOCTL_STORAGE_QUERY_PROPERTY, &Query, Marshal.SizeOf(Query), Allocation.ToPointer(), Desc.Size))
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
                }
                else
                {
                    // Console.WriteLine("[*] Failed to connect with the driver.");
                }

                // 
                // Unload the driver.
                // We don't have ownership of the disk driver, so it doesn't really do anything.
                // 

                if (!Driver.TryUnload())
                {
                    Console.WriteLine("[*] Failed to unload the driver.");
                    goto End;
                }

                // 
                // Dispose the driver interface.
                // 

                End: ;
            }

            return SerialNumber;
        }

        /// <summary>
        /// The IOCTL for storage query property.
        /// </summary>
        private const int IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400;
    }
}
