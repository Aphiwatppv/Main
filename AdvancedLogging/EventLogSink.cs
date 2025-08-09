using System.Diagnostics;

namespace AdvancedLogging
{
    // Windows Event Log sink (optional)
    public sealed class EventLogSink : ILogSink
    {
        private readonly string _source;
        public LogLevel MinimumLevel { get; }
        public EventLogSink(string source, LogLevel min = LogLevel.Warn, bool autoRegister = true)
        {
            _source = source;
            MinimumLevel = min;
            if (autoRegister)
            {
                try
                {
                    if (!EventLog.SourceExists(_source)) EventLog.CreateEventSource(_source, "Application");
                }
                catch { /* permissions may prevent this */ }
            }
        }
        public void Emit(LogEvent e)
        {
            if (e.Level < MinimumLevel) return;
            try
            {
                var type = e.Level >= LogLevel.Error ? EventLogEntryType.Error : (e.Level >= LogLevel.Warn ? EventLogEntryType.Warning : EventLogEntryType.Information);
                var msg = $"{e.Timestamp:u} [{e.Level}] {e.RenderedMessage}\n{e.Exception}";
                EventLog.WriteEntry(_source, msg, type);
            }
            catch { }
        }
        public void Flush() { }
        public void Dispose() { }
    }
}
