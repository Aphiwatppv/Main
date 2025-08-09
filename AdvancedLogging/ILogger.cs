namespace AdvancedLogging
{
    // ============================ Logger ============================
    public interface ILogger : IDisposable
    {
        LogLevel MinimumLevel { get; }
        void Log(LogLevel level, string messageTemplate, Exception exception = null, EventId? eventId = null, IDictionary<string, object> properties = null, string sourceContext = null);
        IDisposable BeginScope(string name, object value);
        IDisposable BeginScope(IDictionary<string, object> properties);

        // Convenience
        void Trace(string messageTemplate, params (string Key, object Value)[] properties);
        void Debug(string messageTemplate, params (string Key, object Value)[] properties);
        void Info(string messageTemplate, params (string Key, object Value)[] properties);
        void Warn(string messageTemplate, params (string Key, object Value)[] properties);
        void Error(string messageTemplate, Exception ex = null, params (string Key, object Value)[] properties);
        void Fatal(string messageTemplate, Exception ex = null, params (string Key, object Value)[] properties);
        void Flush();
    }
}
