namespace Driver.Example
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Driver.Enums;
    using Driver.Logic;

    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static async Task Main()
        {
            var CurrentPath      = Directory.GetCurrentDirectory();
            var CurrentDirectory = new DirectoryInfo(CurrentPath);
            var SystemFile       = new FileInfo(Path.Combine(CurrentPath, "DriverExample2.sys"));
            var LoaderFile       = new FileInfo(Path.Combine(CurrentPath, "Loaders/DriverLoader.exe"));

            if (!SystemFile.Exists)
            {
                foreach (var SysFile in CurrentDirectory.GetFiles("*.sys", SearchOption.AllDirectories))
                {
                    SystemFile   = SysFile;
                }
            }

            if (SystemFile.Exists)
            {
                Console.WriteLine("[*] The driver's file has located at '" + SystemFile.FullName + "'.");

                // ..

                var Driver           = new Driver(new DriverConfig()
                {
                    ServiceName      = "DriverExample2",
                    SymbolicLink     = @"\\.\DriverExample2",
                    DriverFile       = SystemFile,
                    LoadMethod       = DriverLoad.Tdl

                }, LoaderPath: LoaderFile.FullName);

                // ..

                if (Driver.CanConnectTo(@"\\.\DriverExample2"))
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
                                Console.WriteLine("[*] Driver has created the symbolic file.");
                            }
                            else
                            {
                                Console.WriteLine("[*] Failed to open the symbolic file.");
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

                await Task.Delay(500);
            }
            else
            {
                Console.WriteLine("[*] The .sys file is corrupt or missing.");
            }

            Console.ReadKey(true);
        }
    }
}
