using System.Collections.Concurrent;

namespace AdvancedLogging
{
    public sealed class AdvancedLogger : ILogger
    {
        private readonly List<ILogSink> _sinks;
        private readonly List<ILogEnricher> _enrichers;
        private readonly BlockingCollection<LogEvent> _queue;
        private readonly Thread _worker;
        private readonly CancellationTokenSource _cts = new();
        private readonly string _sourceContext;

        public LogLevel MinimumLevel { get; }

        public AdvancedLogger(
            IEnumerable<ILogSink> sinks,
            IEnumerable<ILogEnricher> enrichers = null,
            LogLevel minimumLevel = LogLevel.Info,
            string sourceContext = null)
        {
            _sinks = sinks?.ToList() ?? new List<ILogSink>();
            _enrichers = enrichers?.ToList() ?? new List<ILogEnricher>();
            MinimumLevel = minimumLevel;
            _sourceContext = sourceContext;

            _queue = new BlockingCollection<LogEvent>(new ConcurrentQueue<LogEvent>());
            _worker = new Thread(WorkerLoop) { IsBackground = true, Name = "AdvancedLogger-Worker" };
            _worker.Start();
        }

        public void Log(LogLevel level, string messageTemplate, Exception exception = null, EventId? eventId = null, IDictionary<string, object> properties = null, string sourceContext = null)
        {
            if (level < MinimumLevel) return;

            var ev = new LogEvent
            {
                Timestamp = DateTimeOffset.Now,
                Level = level,
                SourceContext = sourceContext ?? _sourceContext,
                MessageTemplate = messageTemplate ?? string.Empty,
                Exception = exception,
                EventId = eventId,
                Properties = properties != null
                    ? new Dictionary<string, object>(properties, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            };

            // Enrich from scopes and registered enrichers
            ScopeStack.Enrich(ev);
            foreach (var enr in _enrichers) enr.Enrich(ev);

            // Render message (very light template: {Key} from Properties)
            ev.RenderedMessage = Render(ev.MessageTemplate, ev.Properties);

            // Queue for async write
            _queue.Add(ev);
        }

        public IDisposable BeginScope(string name, object value) => BeginScope(new Dictionary<string, object> { [name] = value });
        public IDisposable BeginScope(IDictionary<string, object> properties) => ScopeStack.Push(new Dictionary<string, object>(properties));

        public void Trace(string messageTemplate, params (string Key, object Value)[] properties) => Log(LogLevel.Trace, messageTemplate, null, null, TuplesToDict(properties));
        public void Debug(string messageTemplate, params (string Key, object Value)[] properties) => Log(LogLevel.Debug, messageTemplate, null, null, TuplesToDict(properties));
        public void Info(string messageTemplate, params (string Key, object Value)[] properties) => Log(LogLevel.Info, messageTemplate, null, null, TuplesToDict(properties));
        public void Warn(string messageTemplate, params (string Key, object Value)[] properties) => Log(LogLevel.Warn, messageTemplate, null, null, TuplesToDict(properties));
        public void Error(string messageTemplate, Exception ex = null, params (string Key, object Value)[] properties) => Log(LogLevel.Error, messageTemplate, ex, null, TuplesToDict(properties));
        public void Fatal(string messageTemplate, Exception ex = null, params (string Key, object Value)[] properties) => Log(LogLevel.Fatal, messageTemplate, ex, null, TuplesToDict(properties));

        public void Flush()
        {
            // drain quickly
            var spin = 0;
            while (_queue.Count > 0 && spin < 100) { Thread.Sleep(10); spin++; }
            foreach (var s in _sinks) s.Flush();
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                _queue.CompleteAdding();
                _worker.Join(1000);
            }
            catch { }
            finally
            {
                foreach (var s in _sinks) { try { s.Flush(); s.Dispose(); } catch { } }
            }
        }

        private void WorkerLoop()
        {
            try
            {
                foreach (var e in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    foreach (var s in _sinks)
                    {
                        try { s.Emit(e); } catch { }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                // Last-ditch: write to Debug
                System.Diagnostics.Debug.WriteLine("Logger worker crashed: " + ex);
            }
        }

        private static IDictionary<string, object> TuplesToDict((string Key, object Value)[] kvs)
        {
            if (kvs == null || kvs.Length == 0) return null;
            var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in kvs) d[k] = v;
            return d;
        }

        private static string Render(string template, IDictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            if (properties == null || properties.Count == 0) return template;
            var result = template;
            foreach (var kv in properties)
            {
                var token = "{" + kv.Key + "}";
                if (result.IndexOf(token, StringComparison.Ordinal) >= 0)
                    result = result.Replace(token, kv.Value?.ToString());
            }
            return result;
        }
    }
}
