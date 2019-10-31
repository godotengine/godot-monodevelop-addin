using System.Net;
using Mono.Debugging.Soft;

namespace GodotAddin
{
    public class GodotSoftDebuggerArgs : SoftDebuggerRemoteArgs
    {
        public GodotSoftDebuggerArgs(string appName, IPAddress address, int debugPort, int outputPort) : base(appName, address, debugPort, outputPort)
        {
        }

        public override ISoftDebuggerConnectionProvider ConnectionProvider => null;
    }
}
