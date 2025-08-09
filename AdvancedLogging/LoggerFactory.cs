namespace AdvancedLogging
{
    public static class LoggerFactory
    {
        // Simple factory for class loggers
        public static ILogger CreateFor<T>(ILogger root)
        {
            return new ForwardingLogger(root, typeof(T).FullName);
        }

        private sealed class ForwardingLogger : ILogger
        {
            private readonly ILogger _inner; private readonly string _context;
            public LogLevel MinimumLevel => _inner.MinimumLevel;
            public ForwardingLogger(ILogger inner, string context) { _inner = inner; _context = context; }
            public void Log(LogLevel level, string messageTemplate, Exception exception = null, EventId? eventId = null, IDictionary<string, object> properties = null, string sourceContext = null)
                => _inner.Log(level, messageTemplate, exception, eventId, properties, sourceContext ?? _context);
            public IDisposable BeginScope(string name, object value) => _inner.BeginScope(name, value);
            public IDisposable BeginScope(IDictionary<string, object> properties) => _inner.BeginScope(properties);
            public void Trace(string m, params (string Key, object Value)[] p) => _inner.Trace(m, p);
            public void Debug(string m, params (string Key, object Value)[] p) => _inner.Debug(m, p);
            public void Info(string m, params (string Key, object Value)[] p) => _inner.Info(m, p);
            public void Warn(string m, params (string Key, object Value)[] p) => _inner.Warn(m, p);
            public void Error(string m, Exception ex = null, params (string Key, object Value)[] p) => _inner.Error(m, ex, p);
            public void Fatal(string m, Exception ex = null, params (string Key, object Value)[] p) => _inner.Fatal(m, ex, p);
            public void Flush() => _inner.Flush();
            public void Dispose() => _inner.Dispose();
        }
    }
}
