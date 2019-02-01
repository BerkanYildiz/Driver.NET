namespace Driver.Example
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using Driver.Example.Handlers;
    using Driver.Logic;
    using Driver.Logic.Enums;

    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static async Task Main()
        {
            var CurrentPath      = Directory.GetCurrentDirectory();
            var CurrentDirectory = new DirectoryInfo(CurrentPath);
            var SystemFile       = new FileInfo(Path.Combine(CurrentPath, "MOy38wi6D4AsExHG0.sys"));
            var LoaderFile       = new FileInfo(Path.Combine(CurrentPath, "Loaders/DriverLoader.exe"));

            /* if (!SystemFile.Exists)
            {
                foreach (var SysFile in CurrentDirectory.GetFiles("*.sys", SearchOption.AllDirectories))
                {
                    SystemFile   = SysFile;
                }
            } */

            if (SystemFile.Exists)
            {
                Console.WriteLine("[*] The driver's file has located at '" + SystemFile.FullName + "'.");
            }

            // ..

            var DriverConfig    = new DriverConfig
            {
                ServiceName     = "MOy38wi6D4AsExHG0",
                DriverFile      = SystemFile,
                LoadMethod      = DriverLoad.Capcom,
                IoMethod        = IoMethod.SharedMemory
            };

            switch (DriverConfig.IoMethod)
            {
                case IoMethod.IoControl:
                {
                    DriverConfig.SymbolicLink = @"\\.\" + DriverConfig.ServiceName;
                    break;
                }

                case IoMethod.SharedMemory:
                {
                    DriverConfig.SymbolicLink = @"\BaseNamedObjects\Global\" + DriverConfig.ServiceName;
                    break;
                }
            }

            var Driver          = new Driver(DriverConfig, LoaderFile.FullName);

            // ..

            if (Driver.CanConnectTo(DriverConfig.SymbolicLink, DriverConfig.IoMethod))
            {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[*] The driver symbolic file is already created.");
                Console.WriteLine("[*] Please make sure you are not loading the driver twice!");
                Console.ResetColor();

                Console.WriteLine();
            }

            // ..

            using (Driver)
            {
                try
                {
                    if (Driver.Load())
                    {
                        Console.WriteLine("[*] Driver has been loaded.");

                        if (Driver.IO.IsConnected)
                        {
                            var Requests = new Requests(Driver);
                            Requests.SetProcId(Process.GetProcessesByName("notepad")[0].Id);

                            while (true)
                            {
                                var Result2 = Requests.GetBaseAddress();
                                // var Result  = Requests.GetMemoryRegion(Result2);

                                /* if (Result.HasValue)
                                {
                                    var Region = Result.Value;

                                    // ..

                                    Console.WriteLine("[*] BaseAddress     : " + Region.BaseAddress);
                                    Console.WriteLine("[*] RegionSize      : " + Region.RegionSize);
                                }
                                else
                                {
                                    Console.WriteLine("[*] No region found.");
                                } */

                                if (Result2 > 0x00)
                                {
                                    Console.WriteLine("[*] ModuleBaseAddr  : 0x" + Result2.ToString("X"));
                                }
                                else
                                {
                                    Console.WriteLine("[*] No base addr found.");
                                }

                                Console.ReadKey(true);
                            }
                        }
                        else
                        {
                            Console.WriteLine("[*] Failed to initialize the IO communication.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[*] Failed to load the driver.");
                    }
                }
                catch (Exception Exception)
                {
                    Console.WriteLine("[*] " + Exception.Message + ".");
                }

                Console.WriteLine();
                Console.WriteLine("[*] Driver->Handle       : 0x" + Driver.IO.Handle?.DangerousGetHandle().ToString("X").PadLeft(8, '0'));
                Console.WriteLine("[*] Driver->IsLoaded     : " + Driver.IsLoaded);
                Console.WriteLine("[*] Driver->IsConnected  : " + Driver.IO.IsConnected);
                Console.WriteLine("[*] Driver->IsDisposed   : " + Driver.IsDisposed);

                Console.ReadKey(true);
            }

            Console.ReadKey(true);
        }
    }
}
