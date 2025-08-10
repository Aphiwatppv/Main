using AdvancedLogging;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConnection.Services
{
    public sealed class DapperDataAccess : IDapperDataAccess
    {
        private readonly IConnectService _connect;
        private readonly ILogger _log;

        public DapperDataAccess(IConnectService connect, ILogger logger)
        {
            _connect = connect ?? throw new ArgumentNullException(nameof(connect));
            _log = LoggerFactory.CreateFor<DapperDataAccess>(logger ?? throw new ArgumentNullException(nameof(logger)));
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using (_log.BeginScope(new Dictionary<string, object> { ["op"] = "query", ["sqlHash"] = StableHash(sql) }))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
                    var def = new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct);
                    var rows = await conn.QueryAsync<T>(def).ConfigureAwait(false);
                    var list = rows.AsList();
                    sw.Stop();
                    _log.Info("Query OK ({Rows} rows, {Ms} ms)", ("Rows", list.Count), ("Ms", sw.ElapsedMilliseconds));
                    _log.Debug("Params: {Names}", ("Names", string.Join(",", GetParamNames(param))));
                    return list;
                }
                catch (OperationCanceledException)
                {
                    sw.Stop();
                    _log.Warn("Query canceled after {Ms} ms", ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _log.Error("Query failed after {Ms} ms", ex, ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
            }
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using (_log.BeginScope(new Dictionary<string, object> { ["op"] = "querySingle", ["sqlHash"] = StableHash(sql) }))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
                    var def = new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct);
                    var item = await conn.QuerySingleOrDefaultAsync<T>(def).ConfigureAwait(false);
                    sw.Stop();
                    _log.Info("QuerySingle OK (found={Found}, {Ms} ms)", ("Found", item is not null), ("Ms", sw.ElapsedMilliseconds));
                    _log.Debug("Params: {Names}", ("Names", string.Join(",", GetParamNames(param))));
                    return item;
                }
                catch (OperationCanceledException)
                {
                    sw.Stop();
                    _log.Warn("QuerySingle canceled after {Ms} ms", ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _log.Error("QuerySingle failed after {Ms} ms", ex, ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
            }
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using (_log.BeginScope(new Dictionary<string, object> { ["op"] = "execute", ["sqlHash"] = StableHash(sql) }))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
                    var def = new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct);
                    var affected = await conn.ExecuteAsync(def).ConfigureAwait(false);
                    sw.Stop();
                    _log.Info("Execute OK (affected={Rows}, {Ms} ms)", ("Rows", affected), ("Ms", sw.ElapsedMilliseconds));
                    _log.Debug("Params: {Names}", ("Names", string.Join(",", GetParamNames(param))));
                    return affected;
                }
                catch (OperationCanceledException)
                {
                    sw.Stop();
                    _log.Warn("Execute canceled after {Ms} ms", ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _log.Error("Execute failed after {Ms} ms", ex, ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default)
        {
            using (_log.BeginScope(new Dictionary<string, object> { ["op"] = "scalar", ["sqlHash"] = StableHash(sql) }))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    using var conn = await _connect.OpenAsync(ct).ConfigureAwait(false);
                    var def = new CommandDefinition(sql, param, commandType: commandType, commandTimeout: commandTimeout, cancellationToken: ct);
                    var value = await conn.ExecuteScalarAsync<T>(def).ConfigureAwait(false);
                    sw.Stop();
                    _log.Info("Scalar OK ({Ms} ms)", ("Ms", sw.ElapsedMilliseconds));
                    _log.Debug("Params: {Names}", ("Names", string.Join(",", GetParamNames(param))));
                    return value;
                }
                catch (OperationCanceledException)
                {
                    sw.Stop();
                    _log.Warn("Scalar canceled after {Ms} ms", ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _log.Error("Scalar failed after {Ms} ms", ex, ("Ms", sw.ElapsedMilliseconds));
                    throw;
                }
            }
        }

        // -------- helpers --------
        private static IEnumerable<string> GetParamNames(object? param)
        {
            if (param == null) yield break;

            if (param is DynamicParameters dp)
            {
                foreach (var n in dp.ParameterNames) yield return n;
                yield break;
            }

            // anonymous object or POCO
            var props = param.GetType().GetProperties();
            foreach (var p in props) yield return p.Name;
        }

        private static int StableHash(string s)
        {
            unchecked
            {
                int h = 23;
                for (int i = 0; i < s.Length; i++) h = h * 31 + s[i];
                return h;
            }
        }
    }
}
