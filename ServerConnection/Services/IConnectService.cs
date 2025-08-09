using ServerConnection.Model;
using System.Data;

namespace ServerConnection.Services
{
    public interface IConnectService
    {
        int ConnectionTimeoutSeconds { get; set; }
        ServerInfo Current { get; }
        int MaxPoolSize { get; set; }
        int MinPoolSize { get; set; }
        bool Pooling { get; set; }

        string BuildConnectionString();
        IDbConnection CreateConnection();
        void Dispose();
        Task<IDbConnection> OpenAsync(CancellationToken ct = default);
        void UpdateServer(ServerInfo serverInfo);
    }
}