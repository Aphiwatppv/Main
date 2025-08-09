namespace AdvancedLogging
{
    public interface ILogSink : IDisposable
    {
        LogLevel MinimumLevel { get; }
        void Emit(LogEvent e);
        void Flush();
    }
}
