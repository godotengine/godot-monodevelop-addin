using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GodotAddin.Debugging;
using GodotAddin.GodotMessaging;
using GodotCompletionProviders;
using GodotTools.IdeMessaging;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace GodotAddin
{
    [ExportProjectModelExtension]
    public class GodotProjectExtension : DotNetProjectExtension, IDisposable
    {
        private static readonly SolutionItemRunConfiguration[] RunConfigurations =
        {
            new ProjectRunConfiguration("Play in Editor"),
            new ProjectRunConfiguration("Launch"),
            new ProjectRunConfiguration("Attach")
        };

        private static readonly ExecutionType[] ExecutionTypes =
        {
            ExecutionType.PlayInEditor,
            ExecutionType.Launch,
            ExecutionType.Attach
        };

        public Client GodotMessagingClient { get; private set; }
        public MonoDevelopLogger Logger { get; } = new MonoDevelopLogger();

        private static SolutionItemRunConfiguration GetRunConfiguration(ExecutionType type)
        {
            switch (type)
            {
                case ExecutionType.PlayInEditor:
                    return RunConfigurations[0];
                case ExecutionType.Launch:
                    return RunConfigurations[1];
                case ExecutionType.Attach:
                    return RunConfigurations[2];
                default:
                    throw new NotSupportedException();
            }
        }

        private void OnClientConnected()
        {
            // If the setting is not yet assigned any value, set it to the currently connected Godot editor path
            if (string.IsNullOrEmpty(Settings.GodotExecutablePath))
            {
                string godotPath = GodotMessagingClient?.GodotEditorExecutablePath;
                if (!string.IsNullOrEmpty(godotPath) && File.Exists(godotPath))
                    Settings.GodotExecutablePath.Value = godotPath;
            }
        }

        protected override void OnItemReady()
        {
            base.OnItemReady();

            if (!IsGodotProject())
                return;

            string godotProjectDir = Path.GetDirectoryName(GetGodotProjectPath());
            LoggingService.LogInfo($"Godot project directory is: {godotProjectDir}");

            try
            {
                string DetermineIdentity() => // TODO: Proper detection of whether we are running on VSMac or MD
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "VisualStudioForMac" : "MonoDevelop";

                GodotMessagingClient?.Dispose();
                GodotMessagingClient = new Client(DetermineIdentity(), godotProjectDir, new MessageHandler(), new MonoDevelopLogger());
                GodotMessagingClient.Connected += OnClientConnected;
                GodotMessagingClient.Start();

                BaseCompletionProvider.Context = new GodotProviderContext(this);
            }
            catch (Exception e)
            {
                LoggingService.LogError("Exception when initializing Godot Ide Client", e);
            }
        }

        protected override IEnumerable<SolutionItemRunConfiguration> OnGetRunConfigurations(OperationContext ctx)
        {
            if (IsGodotProject())
                return RunConfigurations;

            return base.OnGetRunConfigurations(ctx);
        }

        protected override ExecutionCommand OnCreateExecutionCommand(ConfigurationSelector configSel, DotNetProjectConfiguration configuration, ProjectRunConfiguration runConfiguration)
        {
            if (IsGodotProject())
            {
                var runConfigurationIndex = RunConfigurations.IndexOf(runConfiguration);

                if (runConfigurationIndex == -1)
                    LoggingService.LogError($"Unexpected RunConfiguration {runConfiguration.Id} {runConfiguration.GetType().FullName}");

                var executionType = ExecutionTypes[runConfigurationIndex];

                if (executionType == ExecutionType.PlayInEditor && !GodotMessagingClient.IsConnected)
                    LoggingService.LogError($"Cannot launch editor player because the Godot Ide Client is not connected");

                string godotProjectPath = GetGodotProjectPath();

                return new GodotExecutionCommand(
                    godotProjectPath,
                    executionType,
                    Path.GetDirectoryName(godotProjectPath),
                    GodotMessagingClient
                );
            }

            return base.OnCreateExecutionCommand(configSel, configuration, runConfiguration);
        }

        private string GetGodotProjectPath()
        {
            return Path.Combine(Path.GetDirectoryName(Project.ParentSolution.FileName), "project.godot");
        }

        private bool? _cachedIsGodotProject;

        private bool IsGodotProject()
        {
            if (!_cachedIsGodotProject.HasValue)
                _cachedIsGodotProject = File.Exists(GetGodotProjectPath());
            return _cachedIsGodotProject.Value;
        }

        protected override ProjectFeatures OnGetSupportedFeatures()
        {
            var features = base.OnGetSupportedFeatures();
            if (IsGodotProject())
                features |= ProjectFeatures.Execute;
            return features;
        }

        protected override bool OnGetCanExecute(ExecutionContext context, ConfigurationSelector configuration, SolutionItemRunConfiguration runConfiguration)
        {
            if (IsGodotProject())
            {
                if (runConfiguration == GetRunConfiguration(ExecutionType.PlayInEditor))
                {
                    // 'Play in Editor' requires the Godot Ide Client to be connected to the server and
                    // the selected run configuration to be 'Debug' (editor/editor player configuration).
                    if (!GodotMessagingClient.IsConnected || IdeApp.Workspace.ActiveConfigurationId != "Debug")
                        return false;
                }

                return true;
            }

            return base.OnGetCanExecute(context, configuration, runConfiguration);
        }

        protected override async Task OnExecuteCommand(ProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configuration, ExecutionCommand executionCommand)
        {
            if (executionCommand is GodotExecutionCommand godotCmd)
            {
                if (godotCmd.ExecutionType == ExecutionType.Launch)
                {
                    if (!File.Exists(Settings.GodotExecutablePath))
                    {
                        // Delay for 1 sec so it's not overriden by build message
                        await Task.Delay(1000);
                        monitor.ReportError(GettextCatalog.GetString($"Godot executable \"{Settings.GodotExecutablePath.Value}\" not found. Update Godot executable setting in perferences."));
                        return;
                    }
                }
            }

            await base.OnExecuteCommand(monitor, context, configuration, executionCommand);
        }

        public override void Dispose()
        {
            base.Dispose();

            GodotMessagingClient?.Dispose();
        }
    }
}
