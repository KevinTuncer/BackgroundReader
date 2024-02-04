using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BackgroundReader
{
    internal class Autostart
    {
        public const string registryPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static string? GetAppName(bool fileName = false)
        {
            if(fileName)
            {
                return System.IO.Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            } 
            else
            {
                return System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            }
        }

        private static string? GetPath()
        {
            return System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        }

        public static void ToggleAutostart(bool enable, string? path, string? name)
        {
            string appName = name ?? GetAppName() ?? "";
            string filePath = GetPath() + '\\' + GetAppName(true) + ".exe";
            var key = Registry.CurrentUser.OpenSubKey(path ?? registryPath, true);

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
