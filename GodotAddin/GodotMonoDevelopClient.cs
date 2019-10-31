using System;
using System.Threading;
using GodotTools.IdeConnection;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace GodotAddin
{
    public class GodotMonoDevelopClient : GodotIdeClient
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

        public GodotMonoDevelopClient(string projectMetadataDir) : base(projectMetadataDir)
        {
            Logger = new MonoDevelopLogger();
        }

        public string GodotEditorExecutablePath => GodotIdeMetadata.EditorExecutablePath;

        private static void DispatchToGuiThread(Action action)
        {
            var d = new SendOrPostCallback((target) => action());
            DispatchService.SynchronizationContext.Send(d, null);
        }

        protected override void OpenFile(string file)
        {
            OpenFile(file, line: 0, column: 0);
        }

        protected override void OpenFile(string file, int line)
        {
            OpenFile(file, line, column: 0);
        }

        protected override void OpenFile(string file, int line, int column)
        {

            DispatchToGuiThread(() =>
            {
                var fileOpenInfo = new FileOpenInformation(new FilePath(file),
                    project: null /* autodetect */,
                    line: line,
                    column: column,
                    options: OpenDocumentOptions.Default
                );

                IdeApp.OpenFiles(new[] { fileOpenInfo });

                // Make the Ide window grab focus
                IdeApp.Workbench.Present();
            });
        }

        public bool SendPlay()
        {
            return WriteMessage(new Message("Play"));
        }

        public bool SendPlay(string debuggerHost, int debuggerPort)
        {
            return WriteMessage(new Message("Play", debuggerHost, debuggerPort.ToString()));
        }

        public bool SendReloadScripts()
        {
            return WriteMessage(new Message("ReloadScripts"));
        }
    }
}
