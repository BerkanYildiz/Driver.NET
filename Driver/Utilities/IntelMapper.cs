namespace Driver.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    using Driver.Native;

    public static class IntelMapper
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool map_driver(byte[] SysBuffer, int SysBufferLength, string FirstEventName, string SecondEventName, int ProcessId, ulong ProcessAddr);

        public static string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the driver.
        /// </summary>
        public static bool LoadDriver(string SysFileName)
        {
            if (!File.Exists(SysFileName))
            {
                Log.Error(typeof(IntelMapper), "Error, the file to map does not exist !");
                Console.WriteLine("[*] {7} : Sys file does not exist.");
                return false;
            }

            if (!File.Exists(Path))
            {
                Log.Error(typeof(IntelMapper), "Error, the file that maps does not exist !");
                Console.WriteLine("[*] {8} : Mapper file does not exist.");
                return false;
            }

            var SysFile                     = new FileInfo(SysFileName);
            var MapperFile                  = new FileInfo(Path);

            if (MapperFile.Exists && SysFile.Exists)
            {
                var SysFilePath             = SysFile.FullName;

                if (SysFilePath.Contains(" "))
                {
                    SysFilePath             = "\"" + SysFilePath + "\"";
                }

                var Proc                    = Process.Start(new ProcessStartInfo(MapperFile.FullName, SysFilePath)
                {
                    UseShellExecute         = false,
                    CreateNoWindow          = true,
                    RedirectStandardError   = true,
                    RedirectStandardOutput  = true,
                    WindowStyle             = ProcessWindowStyle.Hidden
                });

                if (Proc == null)
                {
                    Console.WriteLine("[*] {9} : Unable to start the mapper.");
                    return false;
                }

                var Output = Proc.StandardOutput.ReadToEnd();

                if (!Proc.WaitForExit(10000))
                {
                    Console.Write("[*] {0} : " + Environment.NewLine + Output);
                    Log.Warning(typeof(IntelMapper), "Error, mapper disable timed out !");
                    return false;
                }

                if (Output.Contains("[-]"))
                {
                    Console.Write("[*] {1} : " + Environment.NewLine + Output);
                    Log.Warning(typeof(IntelMapper), "Error, mapper failed to map the driver !");
                    return false;
                }

                if (Output.Contains("[/]"))
                {
                    Console.Write("[*] {2} : " + Environment.NewLine + Output);
                    Log.Warning(typeof(IntelMapper), "Warning, mapper threw security warnings !");
                }

                return true;
            }
            else
            {
                Log.Error(typeof(IntelMapper), "Loader or mapper file does not exist !");
                Console.WriteLine("[*] {3} Loader or mapper does not exist.");
            }

            return false;
        }

        /// <summary>
        /// Loads the driver.
        /// </summary>
        public static bool LoadDriver(string SysFileName, int ProcessId, ulong ProcessAddr, string FirstEventName, string SecondEventName)
        {
            if (!File.Exists(SysFileName))
            {
                Log.Error(typeof(IntelMapper), "Error, the file to map does not exist !");
                return false;
            }

            if (!File.Exists(Path))
            {
                Log.Error(typeof(IntelMapper), "Error, the file that maps does not exist !");
                return false;
            }

            var SysFile                     = new FileInfo(SysFileName);
            var MapperFile                  = new FileInfo(Path);

            if (MapperFile.Exists && SysFile.Exists)
            {
                var SysFilePath             = SysFile.FullName;

                if (SysFilePath.Contains(" "))
                {
                    SysFilePath             = "\"" + SysFilePath + "\"";
                }

                var Proc                    = Process.Start(new ProcessStartInfo(MapperFile.FullName, string.Join(" ", SysFilePath, ProcessId.ToString(), ProcessAddr.ToString(), FirstEventName, SecondEventName))
                {
                    UseShellExecute         = false,
                    CreateNoWindow          = true,
                    RedirectStandardError   = true,
                    RedirectStandardOutput  = true,
                    WindowStyle             = ProcessWindowStyle.Hidden
                });

                if (Proc == null)
                {
                    Console.WriteLine("[*] {9b} : Unable to start the mapper.");
                    return false;
                }

                var Output = Proc.StandardOutput.ReadToEnd();

                if (!Proc.WaitForExit(10000))
                {
                    Console.Write("[*] {0b} : " + Environment.NewLine + Output);
                    Log.Warning(typeof(IntelMapper), "Error, mapper disable timed out !");
                    return false;
                }

                if (Output.Contains("[-]"))
                {
                    Console.Write("[*] {1b} : " + Environment.NewLine + Output);
                    Log.Warning(typeof(IntelMapper), "Error, mapper failed to map the driver !");
                    return false;
                }

                if (Output.Contains("[/]"))
                {
                    Console.Write("[*] {2b} : " + Environment.NewLine + Output);
                    Log.Warning(typeof(IntelMapper), "Warning, mapper threw security warnings !");
                }

                return true;
            }
            else
            {
                Log.Error(typeof(IntelMapper), "Loader or mapper file does not exist !");
                Console.WriteLine("[*] {3b} Loader or mapper does not exist.");
            }

            return false;
        }

        /// <summary>
        /// Loads the driver.
        /// </summary>
        public static bool LoadDriver(byte[] SysFile, int ProcessId, ulong ProcessAddr, string FirstEventName, string SecondEventName)
        {
            if (!File.Exists(Path))
            {
                Log.Error(typeof(IntelMapper), "Error, the file that maps does not exist !");
                return false;
            }

            var MapperPtr = WinApi.LoadLibrary(Path);

            if (MapperPtr == IntPtr.Zero)
            {
                Log.Error(typeof(IntelMapper), "Error, unable to load the mapper !");
                return false;
            }

            var FunctionPtr = WinApi.GetProcAddress(MapperPtr, "map_driver");

            if (FunctionPtr == IntPtr.Zero)
            {
                Log.Error(typeof(IntelMapper), "Error, unable to import the mapper !");
                return false;
            }

            var Function = Marshal.GetDelegateForFunctionPointer<map_driver>(FunctionPtr);

            if (Function == null)
            {
                Log.Error(typeof(IntelMapper), "Error, unable to marshal the mapper !");
                return false;
            }

            var HasBeenMapped = Function(SysFile, SysFile.Length, FirstEventName, SecondEventName, ProcessId, ProcessAddr);

            WinApi.FreeLibrary(MapperPtr);

            if (HasBeenMapped)
            {
                return true;
            }
            else
            {
                Log.Error(typeof(IntelMapper), "Error, unexpected error when mapping the driver !");
            }

            return false;
        }
    }
}