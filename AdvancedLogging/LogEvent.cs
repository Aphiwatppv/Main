namespace AdvancedLogging
{
    // ============================ Primitives ============================
    public sealed class LogEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string SourceContext { get; set; }
        public string MessageTemplate { get; set; }
        public string RenderedMessage { get; set; }
        public Exception Exception { get; set; }
        public EventId? EventId { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
}
