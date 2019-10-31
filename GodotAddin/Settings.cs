using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoDevelop.Core;

namespace GodotAddin
{
    public static class Settings
    {
        private static string DetermineDefaultGodotPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "/Applications/Godot_mono.app/Contents/MacOS/Godot";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? string.Empty, "Godot_x11");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty, "Godot_win32.exe");

            return string.Empty;
        }

        public static readonly ConfigurationProperty<string> GodotExecutablePath =
            ConfigurationProperty.Create("Godot.GodotExecutable", DetermineDefaultGodotPath());

        public static readonly ConfigurationProperty<bool> AlwaysUseConfiguredExecutable =
            ConfigurationProperty.Create("Godot.AlwaysUseConfiguredExecutable", false);
    }
}
