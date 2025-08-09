namespace AdvancedLogging
{
    // Optional event id (like Microsoft.Extensions.Logging)
    public readonly struct EventId
    {
        public int Id { get; }
        public string Name { get; }
        public EventId(int id, string name = null) { Id = id; Name = name ?? ""; }
        public override string ToString() => string.IsNullOrEmpty(Name) ? Id.ToString() : $"{Id}:{Name}";
    }
}
