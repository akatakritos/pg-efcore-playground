using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using StackExchange.Profiling;

namespace Demo.Api.Data
{
    public interface IDatabase
    {
        public DbConnection GetConnection();
    }

    public class PostgresDatabase : IDatabase
    {
        private readonly string _connectionString;

        public PostgresDatabase(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public DbConnection GetConnection()
        {
            DbConnection connection = new NpgsqlConnection(_connectionString);
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection, MiniProfiler.Current);
        }
    }

    public static class DatabaseExtensions
    {
        public static async Task<IDbConnection> GetOpenConnection(this IDatabase db, CancellationToken cancellationToken = default)
        {
            var connection = db.GetConnection();
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
