namespace Driver.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class Turla
    {
        public static string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Disables the security.
        /// </summary>
        public static bool LoadDriver(string DriverPath)
        {
            if (!File.Exists(DriverPath))
            {
                Log.Error(typeof(Capcom), "Error, the file to map does not exist !");
                return false;
            }

            var TurlaFile = new FileInfo(Path);

            if (TurlaFile.Exists)
            {
                var Proc = Process.Start(new ProcessStartInfo(TurlaFile.FullName, DriverPath)
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
                    Console.Write(Output);
                    Log.Warning(typeof(DSEFix), "Warning, Turla disable timed out !");
                    return false;
                }

                if (Output.Contains("failure") || Output.Contains("Error"))
                {
                    Console.Write(Output);
                    Log.Warning(typeof(DSEFix), "Turla failed to load the vulnerable driver !");
                    return false;
                }

                return true;
            }
            else
            {
                Log.Error(typeof(Turla), "Turla file does not exist !");
            }

            return false;
        }
    }
}