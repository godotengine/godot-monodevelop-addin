using System;
using MonoDevelop.Core;

namespace GodotAddin
{
    public class MonoDevelopLogger : GodotTools.IdeMessaging.ILogger, GodotCompletionProviders.ILogger
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
}
