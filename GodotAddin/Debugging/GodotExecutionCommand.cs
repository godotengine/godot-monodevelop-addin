using GodotTools.IdeMessaging;
using MonoDevelop.Core.Execution;

namespace GodotAddin.Debugging
{
    public class GodotExecutionCommand : ExecutionCommand
    {
        public GodotExecutionCommand(string godotProjectPath, ExecutionType executionType,
            string workingDirectory, Client godotIdeClient)
        {
            GodotProjectPath = godotProjectPath;
            ExecutionType = executionType;
            WorkingDirectory = workingDirectory;
            GodotIdeClient = godotIdeClient;
        }

        public string GodotProjectPath { get; }
        public ExecutionType ExecutionType { get; }
        public string WorkingDirectory { get; }
        public Client GodotIdeClient { get; }
    }

    public enum ExecutionType
    {
        PlayInEditor,
        Launch,
        Attach
    }
}
