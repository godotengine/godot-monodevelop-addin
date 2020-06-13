using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger;
using MonoDevelop.Ide;

namespace GodotAddin.Debugging
{
    public class GodotDebuggerEngine : DebuggerEngineBackend
    {
        public GodotDebuggerEngine()
        {
            IdeApp.ProjectOperations.EndBuild += OnProjectOperationsEndBuild;
        }

        private void OnProjectOperationsEndBuild(object sender, MonoDevelop.Projects.BuildEventArgs args)
        {
            foreach (var session in DebuggingService.GetSessions())
            {
                if (session is GodotDebuggerSession godotSession)
                    godotSession.SendReloadScripts();
            }
        }

        public override bool CanDebugCommand(ExecutionCommand cmd)
        {
            return cmd is GodotExecutionCommand;
        }

        public override DebuggerStartInfo CreateDebuggerStartInfo(ExecutionCommand cmd)
        {
            var godotCmd = (GodotExecutionCommand)cmd;
            var godotProjectPath = godotCmd.GodotProjectPath;

            int attachPort = 23685; // Default if not modified

            // Try read the debugger agent port from the 'project.godot' file
            if (File.Exists(godotProjectPath))
            {
                // [mono] "debugger_agent/port"
                var regex = new Regex(@"debugger_agent/port=([0-9]+)");
                foreach (string line in File.ReadAllLines(godotProjectPath))
                {
                    var match = regex.Match(line);

                    if (match.Success)
                    {
                        attachPort = int.Parse(match.Groups[1].Value);
                        break;
                    }
                }
            }

            SoftDebuggerRemoteArgs args;

            if (godotCmd.ExecutionType != ExecutionType.Attach)
                args = new SoftDebuggerListenArgs("Godot", IPAddress.Loopback, 0);
            else
                args = new SoftDebuggerConnectArgs("Godot", IPAddress.Loopback, attachPort);

            return new GodotDebuggerStartInfo(godotCmd, args)
            {
                WorkingDirectory = godotCmd.WorkingDirectory
            };
        }

        public override DebuggerSession CreateSession()
        {
            return new GodotDebuggerSession();
        }

        public override bool IsDefaultDebugger(ExecutionCommand cmd)
        {
            return true;
        }
    }
}
