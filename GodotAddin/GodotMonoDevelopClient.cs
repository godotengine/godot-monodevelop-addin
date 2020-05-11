using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GodotTools.IdeMessaging;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace GodotAddin
{
    public class GodotMonoDevelopClient : DefaultClient
    {
        private class MonoDevelopLogger : ILogger
        {
            public void LogDebug(string message)
            {
                LoggingService.LogDebug(message);
            }

            public void LogInfo(string message)
            {
                LoggingService.LogInfo(message);
            }

            public void LogWarning(string message)
            {
                LoggingService.LogWarning(message);
            }

            public void LogError(string message)
            {
                LoggingService.LogError(message);
            }

            public void LogError(string message, Exception e)
            {
                LoggingService.LogError(message, e);
            }
        }

        private static string DetermineIdentity() => // TODO: Proper detection of whether we are running on VSMac or MD
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "VisualStudioForMac" : "MonoDevelop";

        public GodotMonoDevelopClient(string godotProjectDir)
            : base(DetermineIdentity(), godotProjectDir, new MonoDevelopLogger())
        {
        }

        public string GodotEditorExecutablePath => GodotIdeMetadata.EditorExecutablePath;

        private static void DispatchToGuiThread(Action action)
        {
            var d = new SendOrPostCallback((target) => action());
            DispatchService.SynchronizationContext.Send(d, null);
        }

        protected override async Task OpenFile(string file)
        {
            await OpenFile(file, line: 0, column: 0);
        }

        protected override async Task OpenFile(string file, int line)
        {
            await OpenFile(file, line, column: 0);
        }

        protected override Task OpenFile(string file, int line, int column)
        {
            DispatchToGuiThread(() =>
            {
                var fileOpenInfo = new FileOpenInformation(new FilePath(file),
                    project: null /* autodetect */,
                    line: line,
                    column: column,
                    options: OpenDocumentOptions.Default
                );

                IdeApp.OpenFiles(new[] {fileOpenInfo});

                // Make the Ide window grab focus
                IdeApp.Workbench.Present();
            });
            return Task.CompletedTask;
        }

        public Task<bool> SendPlay()
        {
            return WriteMessage(new Message("Play"));
        }

        public Task<bool> SendPlay(string debuggerHost, int debuggerPort)
        {
            return WriteMessage(new Message("Play", debuggerHost, debuggerPort.ToString()));
        }

        public Task<bool> SendReloadScripts()
        {
            return WriteMessage(new Message("ReloadScripts"));
        }
    }
}
