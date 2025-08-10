using AdvancedLogging;
using Oracle.ManagedDataAccess.Client;
using ServerConnection.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConnection.Services
{
    public sealed class ConnectService : IConnectService
    {
        private readonly object _lock = new();
        private ServerInfo _serverInfo;
        private readonly ILogger _log;

        // Optional knobs
        public bool Pooling { get; set; } = true;
        public int ConnectionTimeoutSeconds { get; set; } = 15;
        public int MinPoolSize { get; set; } = 1;
        public int MaxPoolSize { get; set; } = 50;

        public ServerInfo Current => _serverInfo;

        public ConnectService(ServerInfo serverInfo, ILogger logger)
        {
            _serverInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
            _log = LoggerFactory.CreateFor<ConnectService>(logger ?? throw new ArgumentNullException(nameof(logger)));

            _log.Info("ConnectService initialized for {User}@{Tns}",
                ("User", _serverInfo.Username), ("Tns", _serverInfo.TNS));
        }

        public void UpdateServer(ServerInfo serverInfo)
        {
            if (serverInfo == null) throw new ArgumentNullException(nameof(serverInfo));
            lock (_lock)
            {
                _log.Info("Switching server from {OldUser}@{OldTns} to {NewUser}@{NewTns}",
                    ("OldUser", _serverInfo.Username), ("OldTns", _serverInfo.TNS),
                    ("NewUser", serverInfo.Username), ("NewTns", serverInfo.TNS));
                _serverInfo = serverInfo;
            }
        }

        public string BuildConnectionString()
        {
            var csb = new OracleConnectionStringBuilder
            {
                UserID = _serverInfo.Username,
                Password = _serverInfo.Password, // NOT logged
                DataSource = _serverInfo.TNS,
                PersistSecurityInfo = false,
                Pooling = Pooling
            };
            if (Pooling) { csb.MinPoolSize = MinPoolSize; csb.MaxPoolSize = MaxPoolSize; }

            var raw = csb.ToString();
            if (ConnectionTimeoutSeconds > 0)
                raw += $";Connection Timeout={ConnectionTimeoutSeconds}";

            _log.Debug("Built connection string for {User}@{Tns} (Pooling={Pooling}, Min={Min}, Max={Max}, Timeout={Timeout}s)",
                ("User", _serverInfo.Username), ("Tns", _serverInfo.TNS),
                ("Pooling", Pooling), ("Min", MinPoolSize), ("Max", MaxPoolSize),
                ("Timeout", ConnectionTimeoutSeconds));

            return raw;
        }

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(BuildConnectionString());
        }

        public async Task<IDbConnection> OpenAsync(CancellationToken ct = default)
        {
            using (_log.BeginScope("op", "open-connection"))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var conn = (OracleConnection)CreateConnection();
                    await conn.OpenAsync(ct).ConfigureAwait(false);
                    sw.Stop();
                    _log.Info("Connection opened in {Ms} ms for {User}@{Tns}",
                        ("Ms", sw.ElapsedMilliseconds), ("User", _serverInfo.Username), ("Tns", _serverInfo.TNS));
                    return conn;
                }
                catch (OperationCanceledException)
                {
                    sw.Stop();
                    _log.Warn("OpenAsync canceled after {Ms} ms for {User}@{Tns}",
                        ("Ms", sw.ElapsedMilliseconds), ("User", _serverInfo.Username), ("Tns", _serverInfo.TNS));
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _log.Error("OpenAsync failed after {Ms} ms for {User}@{Tns}", ex,
                        ("Ms", sw.ElapsedMilliseconds), ("User", _serverInfo.Username), ("Tns", _serverInfo.TNS));
                    throw;
                }
            }
        }


        public void Dispose() { /* per-call connections are disposed by callers */ }
    }
}
