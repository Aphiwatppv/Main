using System.Text.Json;
using System.Text;

namespace AdvancedLogging
{
    // ============================ Sinks ============================
    public sealed class RollingFileSink : ILogSink
    {
        private readonly object _lock = new();
        private readonly string _directory;
        private readonly string _fileNamePrefix;  // e.g., "app"
        private readonly bool _writeJson;
        private readonly long _maxBytes;          // roll when exceeded (>0)
        private readonly int _retentionDays;      // delete older than N days (<=0 to disable)
        private StreamWriter _writer;
        private string _currentPath;

        public LogLevel MinimumLevel { get; }

        public RollingFileSink(
            string directory,
            string fileNamePrefix = "app",
            LogLevel minLevel = LogLevel.Info,
            bool writeJson = false,
            long maxBytes = 5 * 1024 * 1024,
            int retentionDays = 14)
        {
            _directory = directory;
            _fileNamePrefix = fileNamePrefix;
            _writeJson = writeJson;
            _maxBytes = maxBytes;
            _retentionDays = retentionDays;
            MinimumLevel = minLevel;

            Directory.CreateDirectory(_directory);
            ApplyRetentionPolicy();
            OpenWriterForToday();
        }

        public void Emit(LogEvent e)
        {
            if (e.Level < MinimumLevel) return;
            lock (_lock)
            {
                if (NeedsRoll(e.Timestamp))
                {
                    OpenWriterForToday();
                }
                var line = _writeJson ? ToJson(e) : ToText(e);
                _writer.WriteLine(line);
                // roll by size if needed
                if (_maxBytes > 0 && _writer.BaseStream.Length >= _maxBytes)
                {
                    OpenWriterForToday(forceNewSequence: true);
                }
            }
        }

        public void Flush()
        {
            lock (_lock) { _writer?.Flush(); }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                try { _writer?.Flush(); _writer?.Dispose(); } catch { }
                _writer = null;
            }
        }

        private void OpenWriterForToday(bool forceNewSequence = false)
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            int seq = 0;
            if (!forceNewSequence && _currentPath != null && File.Exists(_currentPath))
            {
                // keep using current if same day and under size
                var name = Path.GetFileName(_currentPath);
                if (name.Contains(date))
                {
                    // still same day
                    _writer?.Flush();
                    return;
                }
            }

            // close old
            try { _writer?.Flush(); _writer?.Dispose(); } catch { }

            // find next available sequence for today
            string MakePath(int s) => Path.Combine(_directory, $"{_fileNamePrefix}-{date}{(s > 0 ? $"_{s:000}" : "")}.log");
            string path;
            do
            {
                path = MakePath(seq);
                if (!File.Exists(path)) break;
                if (_maxBytes > 0 && new FileInfo(path).Length < _maxBytes && !forceNewSequence) break;
                seq++;
            } while (true);

            _currentPath = path;
            _writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read), new UTF8Encoding(false))
            {
                AutoFlush = true
            };
        }

        private bool NeedsRoll(DateTimeOffset now)
        {
            if (_currentPath == null) return true;
            var name = Path.GetFileName(_currentPath);
            var today = DateTime.Now.ToString("yyyyMMdd");
            return !name.Contains(today);
        }

        private string ToText(LogEvent e)
        {
            var sb = new StringBuilder(256);
            sb.Append(e.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(' ').Append('[').Append(e.Level).Append(']');
            if (!string.IsNullOrEmpty(e.SourceContext)) sb.Append(' ').Append('{').Append(e.SourceContext).Append('}');
            if (e.EventId.HasValue) sb.Append(' ').Append("(Event:").Append(e.EventId.Value.ToString()).Append(')');
            if (e.Properties != null && e.Properties.Count > 0)
            {
                sb.Append(' ').Append('|');
                foreach (var kv in e.Properties)
                {
                    sb.Append(' ').Append(kv.Key).Append('=').Append(kv.Value);
                }
                sb.Append(' ').Append('|');
            }
            sb.Append(' ').Append(e.RenderedMessage ?? e.MessageTemplate);
            if (e.Exception != null)
            {
                sb.AppendLine();
                sb.Append(e.Exception);
            }
            return sb.ToString();
        }

        private string ToJson(LogEvent e)
        {
            var payload = new Dictionary<string, object>
            {
                ["ts"] = e.Timestamp,
                ["level"] = e.Level.ToString(),
                ["source"] = e.SourceContext,
                ["message"] = e.RenderedMessage ?? e.MessageTemplate,
            };
            if (e.EventId.HasValue) payload["eventId"] = e.EventId.Value.ToString();
            if (e.Properties?.Count > 0) payload["props"] = e.Properties;
            if (e.Exception != null) payload["exception"] = e.Exception.ToString();
            return JsonSerializer.Serialize(payload);
        }

        private void ApplyRetentionPolicy()
        {
            if (_retentionDays <= 0) return;
            try
            {
                var cutoff = DateTime.Now.AddDays(-_retentionDays);
                foreach (var f in Directory.EnumerateFiles(_directory, $"{_fileNamePrefix}-*.log"))
                {
                    try
                    {
                        var info = new FileInfo(f);
                        if (info.LastWriteTime < cutoff) info.Delete();
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
