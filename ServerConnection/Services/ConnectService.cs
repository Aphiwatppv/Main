using Oracle.ManagedDataAccess.Client;
using ServerConnection.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConnection.Services
{
    public sealed class ConnectService : IConnectService
    {
        private readonly object _lock = new();
        private ServerInfo _serverInfo;

        // Optional connection string knobs (tweak to your needs)
        public bool Pooling { get; set; } = true;
        public int ConnectionTimeoutSeconds { get; set; } = 15;
        public int MinPoolSize { get; set; } = 1;
        public int MaxPoolSize { get; set; } = 50;

        public ServerInfo Current => _serverInfo;

        public ConnectService(ServerInfo serverInfo)
        {
            _serverInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
        }

        public void UpdateServer(ServerInfo serverInfo)
        {
            if (serverInfo == null) throw new ArgumentNullException(nameof(serverInfo));
            lock (_lock) { _serverInfo = serverInfo; }
        }

        public string BuildConnectionString()
        {
            // Standard Oracle Managed ODP.NET format
            // If you use EZCONNECT like "host:1521/service" put it in TNS.
            var csb = new OracleConnectionStringBuilder
            {
                UserID = _serverInfo.Username,
                Password = _serverInfo.Password,
                DataSource = _serverInfo.TNS,
                PersistSecurityInfo = false,
                Pooling = Pooling
            };

            if (Pooling)
            {
                csb.MinPoolSize = MinPoolSize;
                csb.MaxPoolSize = MaxPoolSize;
            }

            // OracleConnectionStringBuilder doesn’t expose connection timeout,
            // but ODP honors "Connection Timeout" in the raw string.
            // Append it manually if desired:
            var raw = csb.ToString();
            if (ConnectionTimeoutSeconds > 0)
                raw += $";Connection Timeout={ConnectionTimeoutSeconds}";

            return raw;
        }

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(BuildConnectionString());
        }

        public async Task<IDbConnection> OpenAsync(CancellationToken ct = default)
        {
            var conn = (OracleConnection)CreateConnection();
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return conn;
        }


        public void Dispose()
        {
            // nothing to dispose here; per-call connections are disposed by callers
        }
    }
}
