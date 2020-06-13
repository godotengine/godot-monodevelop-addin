using System;
using System.Threading.Tasks;
using GodotCompletionProviders;
using GodotTools.IdeMessaging;
using GodotTools.IdeMessaging.Requests;
using ILogger = GodotCompletionProviders.ILogger;

namespace GodotAddin
{
    internal class GodotProviderContext : IProviderContext
    {
        private readonly GodotProjectExtension _extension;

        public GodotProviderContext(GodotProjectExtension package)
        {
            _extension = package;
        }

        public ILogger GetLogger() => _extension.Logger;

        public bool AreCompletionsEnabledFor(CompletionKind completionKind)
        {
            return completionKind switch
            {
                CompletionKind.NodePaths => Settings.ProvideNodePathCompletions,
                CompletionKind.InputActions => Settings.ProvideInputActionCompletions,
                CompletionKind.ResourcePaths => Settings.ProvideResourcePathCompletions,
                CompletionKind.ScenePaths => Settings.ProvideScenePathCompletions,
                CompletionKind.Signals => Settings.ProvideSignalNameCompletions,
                _ => false
            };
        }

        public bool CanRequestCompletionsFromServer()
        {
            var godotMessagingClient = _extension.GodotMessagingClient;
            return godotMessagingClient != null && godotMessagingClient.IsConnected;
        }

        public async Task<string[]> RequestCompletion(CompletionKind completionKind, string absoluteFilePath)
        {
            var godotMessagingClient = _extension.GodotMessagingClient;

            if (godotMessagingClient == null)
                throw new InvalidOperationException();

            var request = new CodeCompletionRequest { Kind = (CodeCompletionRequest.CompletionKind)completionKind, ScriptFile = absoluteFilePath };
            var response = await godotMessagingClient.SendRequest<CodeCompletionResponse>(request);

            if (response.Status == MessageStatus.Ok)
                return response.Suggestions;

            GetLogger().LogError($"Received code completion response with status '{response.Status}'.");
            return new string[] { };
        }
    }
}
