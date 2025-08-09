using System.Diagnostics;

namespace AdvancedLogging
{
    // ============================ Enrichers ============================
    public sealed class StandardEnricher : ILogEnricher
    {
        private static readonly int _processId = Process.GetCurrentProcess().Id;
        public void Enrich(LogEvent e)
        {
            e.Properties["machine"] = Environment.MachineName;
            e.Properties["user"] = Environment.UserName;
            e.Properties["processId"] = _processId;
            e.Properties["threadId"] = Environment.CurrentManagedThreadId;
        }
    }
}
