namespace Driver.NET.Services
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

    public partial class WindowsService
    {
        /// <summary>
        /// Opens a handle to this service's registry key.
        /// </summary>
        public SafeRegistryHandle OpenRegistryHandle()
        {
            return this.OpenRegistryKey().Handle;
        }

        /// <summary>
        /// Opens a handle to this service's registry key wrapped in a <see cref="RegistryKey"/> instance.
        /// </summary>
        public RegistryKey OpenRegistryKey()
        {
            return Registry.LocalMachine.OpenSubKey(this.RegistryPathRelativeToLocalMachine);
        }

        /// <summary>
        /// Creates the registry key for this service.
        /// </summary>
        public void CreateRegistryKey()
        {
            Registry.LocalMachine.CreateSubKey(this.RegistryPathRelativeToLocalMachine)?.Close();
        }

        /// <summary>
        /// Deletes the registry key for this service.
        /// </summary>
        public void DeleteRegistryKey()
        {
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(this.RegistryPathRelativeToLocalMachine, false);
            }
            catch (Exception)
            {
                // ...
            }
        }

        /// <summary>
        /// Reads and return a registry value from the current service's registry key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ValueName">The name of the value.</param>
        /// <param name="DefaultValue">The default value.</param>
        public T ReadRegistryValue<T>(string ValueName, T DefaultValue = default(T))
        {
            // 
            // Check if the value name is null or empty.
            // 

            if (string.IsNullOrEmpty(ValueName))
            {
                throw new ArgumentNullException(nameof(ValueName), "The registry value name is null or empty.");
            }

            // 
            // Are there any slashes in the value name ?
            // 

            if (ValueName.Any(C => C == '/' || C == '\\'))
            {
                // 
                // Convert all forward slashes '/' to backward slashes '\'.
                // 

                ValueName = ValueName.Replace('/', '\\');

                // 
                // Make sure the value name does not end with a slash.
                // 

                if (ValueName[ValueName.Length - 1] == '\\')
                {
                    // 
                    // Remove the backward slash.
                    // 

                    ValueName = ValueName.Remove(ValueName.Length - 1, 1);
                }

                // 
                // Parse the sub-key name and the real value name.
                // 

                var SplitName = ValueName.Split('\\');
                var SubKeyName = string.Join("\\", SplitName, 0, SplitName.Length - 1);
                var RealValueName = SplitName[SplitName.Length - 1];

                // 
                // We're done, get the value from the registry with the modified parameters.
                // 

                return (T) Registry.GetValue(Path.Combine(this.RegistryPath, SubKeyName), RealValueName, DefaultValue);
            }

            return (T) Registry.GetValue(this.RegistryPath, ValueName, DefaultValue);
        }

        /// <summary>
        /// Writes a registry value to the current service's registry key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ValueName">The name of the value.</param>
        /// <param name="Value">The value.</param>
        /// <param name="ValueKind">The type of the value in registry.</param>
        public void WriteRegistryValue<T>(string ValueName, T Value, RegistryValueKind ValueKind = RegistryValueKind.Unknown)
        {
            // 
            // Check if the value name is null or empty.
            // 

            if (string.IsNullOrEmpty(ValueName))
            {
                throw new ArgumentNullException(nameof(ValueName), "The registry value name is null or empty.");
            }

            // 
            // Are there any slashes in the value name ?
            // 

            if (ValueName.Any(C => C == '/' || C == '\\'))
            {
                // 
                // Convert all forward slashes '/' to backward slashes '\'.
                // 

                ValueName = ValueName.Replace('/', '\\');

                // 
                // Make sure the value name does not end with a slash.
                // 

                if (ValueName[ValueName.Length - 1] == '\\')
                {
                    // 
                    // Remove the backward slash.
                    // 

                    ValueName = ValueName.Remove(ValueName.Length - 1, 1);
                }

                // 
                // Parse the sub-key name and the real value name.
                // 

                var SplitName = ValueName.Split('\\');
                var SubKeyName = string.Join("\\", SplitName, 0, SplitName.Length - 1);
                var RealValueName = SplitName[SplitName.Length - 1];

                // 
                // Make sure the sub-key exists.
                // 

                using (var ServiceKey = Registry.LocalMachine.OpenSubKey(this.RegistryPathRelativeToLocalMachine, true))
                {
                    if (ServiceKey == null)
                    {
                        throw new ArgumentException("The specified registry path does not exist.", nameof(SubKeyName));
                    }

                    ServiceKey.CreateSubKey(SubKeyName)?.Close();
                }

                // 
                // We're done, get the value from the registry with the modified parameters.
                // 

                Registry.SetValue(Path.Combine(this.RegistryPath, SubKeyName), RealValueName, Value, ValueKind);
                return;
            }

            Registry.SetValue(this.RegistryPath, ValueName, Value, ValueKind);
        }
    }
}