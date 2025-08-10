using System.Data;

namespace ServerConnection.Services
{
    public interface IDapperDataAccess
    {
        Task<int> ExecuteAsync(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default);
        Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default);
        Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default);
        Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken ct = default);
    }
}