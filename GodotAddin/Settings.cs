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

        public static readonly ConfigurationProperty<bool> AlwaysUseConfiguredExecutable =
            ConfigurationProperty.Create("Godot.Debugging.AlwaysUseConfiguredExecutable", false);

        public static readonly ConfigurationProperty<string> GodotExecutablePath =
            ConfigurationProperty.Create("Godot.Debugging.GodotExecutable", DetermineDefaultGodotPath());

        public static ConfigurationProperty<bool> ProvideNodePathCompletions { get; set; } =
            ConfigurationProperty.Create("Godot.CodeCompletion.ProvideNodePathCompletions", true);

        public static ConfigurationProperty<bool> ProvideInputActionCompletions { get; set; } =
            ConfigurationProperty.Create("Godot.CodeCompletion.ProvideInputActionCompletions", true);

        public static ConfigurationProperty<bool> ProvideResourcePathCompletions { get; set; } =
            ConfigurationProperty.Create("Godot.CodeCompletion.ProvideResourcePathCompletions", true);

        public static ConfigurationProperty<bool> ProvideScenePathCompletions { get; set; } =
            ConfigurationProperty.Create("Godot.CodeCompletion.ProvideScenePathCompletions", true);

        public static ConfigurationProperty<bool> ProvideSignalNameCompletions { get; set; } =
            ConfigurationProperty.Create("Godot.CodeCompletion.ProvideSignalNameCompletions", true);
    }
}
