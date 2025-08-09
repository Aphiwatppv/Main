namespace AdvancedLogging
{
    // ============================ Builder / Factory ============================
    public sealed class LoggerBuilder
    {
        private readonly List<ILogSink> _sinks = new();
        private readonly List<ILogEnricher> _enrichers = new();
        private LogLevel _minLevel = LogLevel.Info;
        private string _sourceContext;

        public LoggerBuilder MinimumLevel(LogLevel level) { _minLevel = level; return this; }
        public LoggerBuilder WithSourceContext(string source) { _sourceContext = source; return this; }
        public LoggerBuilder EnrichWith(ILogEnricher enricher) { if (enricher != null) _enrichers.Add(enricher); return this; }

        public LoggerBuilder WriteToFile(string directory, string prefix = "app", LogLevel min = LogLevel.Info, bool json = false, long maxBytes = 5 * 1024 * 1024, int retentionDays = 14)
        {
            _sinks.Add(new RollingFileSink(directory, prefix, min, json, maxBytes, retentionDays));
            return this;
        }

        public LoggerBuilder WriteToDebug(LogLevel min = LogLevel.Debug)
        {
            _sinks.Add(new DebugSink(min));
            return this;
        }

        public LoggerBuilder WriteToEventLog(string source, LogLevel min = LogLevel.Warn, bool autoRegister = true)
        {
            _sinks.Add(new EventLogSink(source, min, autoRegister));
            return this;
        }

        public ILogger Build()
        {
            if (_enrichers.All(e => e is not StandardEnricher))
                _enrichers.Add(new StandardEnricher());
            return new AdvancedLogger(_sinks, _enrichers, _minLevel, _sourceContext);
        }
    }
}
