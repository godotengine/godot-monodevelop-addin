using Mono.Debugging.Soft;

namespace GodotAddin.Debugging
{
    public class GodotDebuggerStartInfo : SoftDebuggerStartInfo
    {
        public GodotExecutionCommand GodotCmd { get; }

        public GodotDebuggerStartInfo(GodotExecutionCommand godotCmd, SoftDebuggerRemoteArgs softDebuggerConnectArgs) :
            base(softDebuggerConnectArgs)
        {
            GodotCmd = godotCmd;
        }
    }
}
