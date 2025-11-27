namespace LoggingService
{
    using NLog;

    public class NLogService : ILoggingService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void LogInfo(string message) => Logger.Info(message);

        public void LogError(string message, Exception ex) => Logger.Error(ex, message);
        public void LogWarning(string message) => Logger.Warn(message);
        
        public void LogDebug(string message) =>  Logger.Debug(message); 
    }
}