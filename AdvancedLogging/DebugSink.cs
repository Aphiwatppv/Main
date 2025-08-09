namespace AdvancedLogging
{
    public sealed class DebugSink : ILogSink
    {
        public LogLevel MinimumLevel { get; }
        public DebugSink(LogLevel min = LogLevel.Debug) { MinimumLevel = min; }
        public void Emit(LogEvent e)
        {
            if (e.Level < MinimumLevel) return;
            System.Diagnostics.Debug.WriteLine($"[{e.Level}] {e.RenderedMessage}");
            if (e.Exception != null) System.Diagnostics.Debug.WriteLine(e.Exception);
        }
        public void Flush() { }
        public void Dispose() { }
    }
}
