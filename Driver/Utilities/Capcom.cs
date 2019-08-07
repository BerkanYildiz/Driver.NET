namespace Driver.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class Capcom
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

            var CapcomFile = new FileInfo(Path);

            if (CapcomFile.Exists)
            {
                var Proc = Process.Start(new ProcessStartInfo(CapcomFile.FullName, DriverPath)
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
                    Log.Warning(typeof(Capcom), "Warning, Capcom disable timed out !");
                    return false;
                }

                if (Output.Contains("[-]"))
                {
                    Console.Write(Output);
                    Log.Warning(typeof(Capcom), "Capcom failed to load the driver !");
                    return false;
                }

                if (Output.Contains("[/]"))
                {
                    Console.Write(Output);
                    Log.Warning(typeof(Capcom), "Capcom threw security warnings !");
                    // return false;
                }

                return true;
            }
            else
            {
                Log.Error(typeof(Capcom), "Capcom file does not exist !");
            }

            return false;
        }
    }
}