using MonoDevelop.Core.Execution;

namespace GodotAddin
{
    public class GodotExecutionCommand : ExecutionCommand
    {
        public GodotExecutionCommand(string godotProjectPath, ExecutionType executionType,
            string workingDirectory, GodotMonoDevelopClient godotIdeClient)
        {
            GodotProjectPath = godotProjectPath;
            ExecutionType = executionType;
            WorkingDirectory = workingDirectory;
            GodotIdeClient = godotIdeClient;
        }

        public string GodotProjectPath { get; }
        public ExecutionType ExecutionType { get; }
        public string WorkingDirectory { get; }
        public GodotMonoDevelopClient GodotIdeClient { get; }
    }

    public enum ExecutionType
    {
        PlayInEditor,
        Launch,
        Attach
    }
}
