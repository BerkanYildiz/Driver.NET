namespace Driver.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class DSEFix
    {
        public static string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Disables the security.
        /// </summary>
        public static void DisableSecurity()
        {
            var DSEFile = new FileInfo(Path);

            if (DSEFile.Exists)
            {
                var DSEFix = Process.Start(new ProcessStartInfo(DSEFile.FullName)
                {
                    UseShellExecute         = false,
                    CreateNoWindow          = true,
                    RedirectStandardError   = true,
                    RedirectStandardOutput  = true,
                    WindowStyle             = ProcessWindowStyle.Hidden
                });

                string Output = DSEFix.StandardOutput.ReadToEnd();

                if (Output.Contains("failure"))
                {
                    Log.Warning(typeof(DSEFix), "DSE failed to load the vulnerable driver !");
                }

                if (!DSEFix.WaitForExit(10000))
                {
                    Log.Warning(typeof(DSEFix), "Warning, DSE disable timed out !");
                }
            }
        }

        /// <summary>
        /// Enables the security.
        /// </summary>
        public static void EnableSecurity()
        {
            var DSEFile = new FileInfo(Path);

            if (DSEFile.Exists)
            {
                var DSEFix = Process.Start(new ProcessStartInfo(DSEFile.FullName, "-e")
                {
                    UseShellExecute             = false,
                    CreateNoWindow              = true,
                    RedirectStandardError       = true,
                    RedirectStandardOutput      = true,
                    WindowStyle                 = ProcessWindowStyle.Hidden
                });

                string Output = DSEFix.StandardOutput.ReadToEnd();

                /* if (Output.Contains("failure"))
                {
                    Log.Warning(typeof(DSEFix), "DSE failed to load the vulnerable driver !");
                } */

                if (!DSEFix.WaitForExit(10000))
                {
                    Console.Write(Output);
                    Log.Warning(typeof(DSEFix), "Warning, DSE enable timed out !");
                }

                Log.Info(typeof(DSEFix), "Enabled Patch Guard !");
            }
        }
    }
}