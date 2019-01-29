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
            var SystemFile       = new FileInfo(Path.Combine(CurrentPath, "qdMHLCGQ6jUzlWsbrgGF.sys"));
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

            var Driver           = new Driver(new DriverConfig()
            {
                ServiceName     = "qdMHLCGQ6jUzlWsbrgGF",
                SymbolicLink    = @"\\.\qdMHLCGQ6jUzlWsbrgGF",
                DriverFile      = SystemFile,
                LoadMethod      = DriverLoad.Capcom,
                IoMethod        = IoMethod.SharedMemory

            }, LoaderPath: LoaderFile.FullName);

            // ..

            if (Driver.CanConnectTo(@"\\.\qdMHLCGQ6jUzlWsbrgGF", IoMethod.SharedMemory))
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
                            var Requests     = new Requests(Driver);

                            while (true)
                            {
                                Requests.SetProcId(Process.GetProcessesByName("notepad")[0].Id);

                                var Chrono  = Stopwatch.StartNew();

                                for (int I = 0; I < 100000; I++)
                                {
                                    Requests.GetBaseAddress();
                                }

                                Chrono.Stop();

                                Console.WriteLine("[*] 50,000 requests in " + Chrono.Elapsed.TotalMilliseconds + "ms");

                                var Result  = Requests.GetMemoryRegion(0x82d8800000);
                                var Result2 = Requests.GetBaseAddress();

                                if (Result.HasValue)
                                {
                                    var Region = Result.Value;

                                    // ..

                                    Console.WriteLine("[*] BaseAddress     : " + Region.BaseAddress);
                                    Console.WriteLine("[*] RegionSize      : " + Region.RegionSize);
                                }
                                else
                                {
                                    Console.WriteLine("[*] No region found.");
                                }

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
