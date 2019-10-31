using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace GodotAddin
{
    [ExportProjectModelExtension]
    public class GodotProjectExtension : DotNetProjectExtension, IDisposable
    {
        private static readonly SolutionItemRunConfiguration[] runConfigurations =
        {
            new ProjectRunConfiguration("Play in Editor"),
            new ProjectRunConfiguration("Launch"),
            new ProjectRunConfiguration("Attach")
        };

        private static readonly ExecutionType[] executionTypes =
        {
            ExecutionType.PlayInEditor,
            ExecutionType.Launch,
            ExecutionType.Attach
        };

        private static SolutionItemRunConfiguration GetRunConfiguration(ExecutionType type)
        {
            switch (type)
            {
                case ExecutionType.PlayInEditor:
                    return runConfigurations[0];
                case ExecutionType.Launch:
                    return runConfigurations[1];
                case ExecutionType.Attach:
                    return runConfigurations[2];
                default:
                    throw new NotSupportedException();
            }
        }

        private GodotMonoDevelopClient godotIdeClient;

        protected override void OnItemReady()
        {
            base.OnItemReady();

            if (!IsGodotProject())
                return;

            string godotProjectDir = Path.GetDirectoryName(GetGodotProjectPath());
            string godotProjectMetadataDir = Path.Combine(godotProjectDir, ".mono", "metadata");
            LoggingService.LogInfo($"Godot project directory is: {godotProjectDir}");

            try
            {
                godotIdeClient?.Dispose();
                godotIdeClient = new GodotMonoDevelopClient(godotProjectMetadataDir);
                godotIdeClient.Start();
            }
            catch (Exception e)
            {
                LoggingService.LogError("Exception when initializing Godot Ide Client", e);
            }
        }

        protected override IEnumerable<SolutionItemRunConfiguration> OnGetRunConfigurations(OperationContext ctx)
        {
            if (IsGodotProject())
                return runConfigurations;

            return base.OnGetRunConfigurations(ctx);
        }

        protected override ExecutionCommand OnCreateExecutionCommand(ConfigurationSelector configSel, DotNetProjectConfiguration configuration, ProjectRunConfiguration runConfiguration)
        {
            if (IsGodotProject())
            {
                var runConfigurationIndex = runConfigurations.IndexOf(runConfiguration);

                if (runConfigurationIndex == -1)
                    LoggingService.LogError($"Unexpected RunConfiguration {runConfiguration.Id} {runConfiguration.GetType().FullName}");

                var executionType = executionTypes[runConfigurationIndex];

                if (executionType == ExecutionType.PlayInEditor && !godotIdeClient.IsConnected)
                    LoggingService.LogError($"Cannot launch editor player because the Godot Ide Client is not connected");

                string godotProjectPath = GetGodotProjectPath();

                return new GodotExecutionCommand(
                    godotProjectPath,
                    executionType,
                    Path.GetDirectoryName(godotProjectPath),
                    godotIdeClient
                );
            }

            return base.OnCreateExecutionCommand(configSel, configuration, runConfiguration);
        }

        private string GetGodotProjectPath()
        {
            return Path.Combine(Path.GetDirectoryName(Project.ParentSolution.FileName), "project.godot");
        }

        private bool? cachedIsGodotProject;

        private bool IsGodotProject()
        {
            if (!cachedIsGodotProject.HasValue)
                cachedIsGodotProject = File.Exists(GetGodotProjectPath());
            return cachedIsGodotProject.Value;
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
                    // 'Play in Editor' requires the Godot Ide Client to be connected
                    // to a server and the selected run configuration to be 'Tools'.
                    if (!godotIdeClient.IsConnected || IdeApp.Workspace.ActiveConfigurationId != "Tools")
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

            godotIdeClient?.Dispose();
        }
    }
}
