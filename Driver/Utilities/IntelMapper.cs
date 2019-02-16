namespace Driver.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal static class IntelMapper
    {
        internal static string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the driver.
        /// </summary>
        internal static bool LoadDriver(string SysFileName)
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
        internal static bool LoadDriver(string SysFileName, int ProcessId, ulong ProcessAddr, string FirstEventName, string SecondEventName)
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

                Console.WriteLine(Output);

                return true;
            }
            else
            {
                Log.Error(typeof(IntelMapper), "Loader or mapper file does not exist !");
                Console.WriteLine("[*] {3} Loader or mapper does not exist.");
            }

            return false;
        }
    }
}