using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConnection.Services
{
    public sealed class DapperDataAccess : IDapperDataAccess
    {
        private readonly IConnectService _connect;

        public DapperDataAccess(IConnectService connect) => _connect = connect;

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QueryAsync<T>(new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct))
                             .ConfigureAwait(false);
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
            return await conn.QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct))
                             .ConfigureAwait(false);
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
            return await conn.ExecuteAsync(new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct))
                             .ConfigureAwait(false);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
            return await conn.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct))
                             .ConfigureAwait(false);
        }
    }
}
