namespace Driver.Example
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Driver.Example.Handlers;
    using Driver.Logic;
    using Driver.Logic.Enums;

    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            var DriverConfig        = new DriverConfig
            {
                ServiceName         = "TopKek",
                DriverFile          = new FileInfo("TopKek.sys"),
                LoadMethod          = DriverLoad.IntelMapp90er,
                IoMethod            = IoMethod.SharedMemory,
                SharedMemory        = new DriverConfig.DriverConfigSharedMemory()
                {
                    ProcessId       = Process.GetCurrentProcess().Id,
                    FirstEventName  = @"\BaseNamedObjects\Global\TopKekFirstEvent",
                    SecondEventName = @"\BaseNamedObjects\Global\TopKekSecondEvent",
                }
            };

            switch (DriverConfig.IoMethod)
            {
                case IoMethod.IoControl:
                {
                    DriverConfig.SymbolicLink = @"\\.\" + DriverConfig.ServiceName;
                    break;
                }
            }

            var Driver              = new Driver(DriverConfig, new FileInfo("IntelMapp90er.exe").FullName);

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
                            Console.ReadKey(true);

                            // ..

                            var Requests = new Requests(Driver);

                            for (int I = 0; I < 10; I++)
                            {
                                Requests.SetProcId(Process.GetProcessesByName("notepad")[0].Id);

                                Console.WriteLine("[*] [" + I + "] Base Address : 0x" + Requests.GetBaseAddress().ToString("X"));
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
            }

            Console.ReadKey(true);
        }
    }
}
