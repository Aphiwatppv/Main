using ConfigServices.Model;

namespace ConfigServices.Service
{
    public interface IServerConfigService
    {
        Guid Add(ServerConfig config);
        bool Delete(Guid id);
        bool Edit(Guid id, ServerConfig updated);
        IReadOnlyList<ServerConfig> GetAll();
        ServerConfig? GetById(Guid id);
        IReadOnlyList<ServerConfig> GetByLocation(string location);
        IReadOnlyList<ServerConfig> GetByLocationInstance(string location, string instance);
        ServerConfig? GetByLocationInstanceUsername(string location, string instance, string username);
        ServerConfig GetRequired(Guid id);
        ServerConfigList Load();
        void Save(ServerConfigList data);
        IReadOnlyList<string> GetDistinctLocations(bool sorted = true);
    }
}