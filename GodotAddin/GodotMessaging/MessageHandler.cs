using System;
using System.Threading;
using System.Threading.Tasks;
using GodotTools.IdeMessaging;
using GodotTools.IdeMessaging.Requests;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace GodotAddin.GodotMessaging
{
    public class MessageHandler : ClientMessageHandler
    {
        private static void DispatchToGuiThread(Action action)
        {
            var d = new SendOrPostCallback((target) => action());
            DispatchService.SynchronizationContext.Send(d, null);
        }

        protected override Task<Response> HandleOpenFile(OpenFileRequest request)
        {
            DispatchToGuiThread(() =>
            {
                var fileOpenInfo = new FileOpenInformation(new FilePath(request.File),
                    project: null /* autodetect */,
                    line: request.Line ?? 0,
                    column: request.Column ?? 0,
                    options: OpenDocumentOptions.Default
                );

                IdeApp.OpenFiles(new[] { fileOpenInfo });

                // Make the Ide window grab focus
                IdeApp.Workbench.Present();
            });

            return Task.FromResult<Response>(new OpenFileResponse { Status = MessageStatus.Ok });
        }
    }
}
