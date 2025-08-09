namespace AdvancedLogging
{

    public interface ILogEnricher
    {
        void Enrich(LogEvent e);
    }
}
