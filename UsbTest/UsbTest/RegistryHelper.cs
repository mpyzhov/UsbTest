using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbTest
{
    internal static class RegistryHelper
    {
        private const string StartUpKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string ApplicationName = "UsbTest";

        internal static void SetupApplicationRunAtStartup(bool value)
        {
            using (var registryKey = Registry.CurrentUser.OpenSubKey(StartUpKeyPath, true))
            {
                if (registryKey == null)
                {
                    throw new InvalidOperationException("Error opening registry key with path: " + StartUpKeyPath);
                }

                if (value)
                {
                    var cmd = GetStartupCmd();
                    registryKey.SetValue(ApplicationName, cmd);
                }
                else
                {
                    if (registryKey.GetValue(ApplicationName) != null)
                    {
                        registryKey.DeleteValue(ApplicationName);
                    }
                }
            }
        }

        private static string GetStartupCmd()
        {
            return string.Format("\"{0}\"", GetExecutablePath());
        }

        private static string GetExecutablePath()
        {
            return new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }
    }
}
