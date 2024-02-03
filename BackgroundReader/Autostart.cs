using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BackgroundReader
{
    internal class Autostart
    {
        public const string registryPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static string GetAppName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void ToggleAutostart(bool enable, string? path, string? name)
        {
            var key = Registry.CurrentUser.OpenSubKey(path ?? registryPath, true);
            string filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appName = name ?? GetAppName();

            if (key != null)
            {
                if (enable)
                {
                    key.SetValue(appName, filePath);
                }
                else
                {
                    key.DeleteValue(appName, false);
                }
            }
        }

        private static string? GetAutostartPath()
        {
            var key = Registry.CurrentUser.OpenSubKey(registryPath, false);

            if (key != null)
            {
                var value = key.GetValue(GetAppName()) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        public static bool IsAutostartEnabled()
        {
            return !string.IsNullOrEmpty(GetAutostartPath());
        }
    }
}
