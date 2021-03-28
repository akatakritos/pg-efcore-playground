using System;
using System.Diagnostics;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using Serilog;

namespace Demo.Api.Data.Migrations
{
    public class DbUpMigrator
    {
        private static readonly ILogger _log = Log.ForContext<DbUpMigrator>();

        private readonly string _connectionString;

        public DbUpMigrator(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public static bool IsReRunnableMigration(string filename)
        {
            return filename.StartsWith("Demo.Api.Data.Migrations.R");
        }

        public void Migrate()
        {
            _log.Information("Migrating database");
            var upgrader = DeployChanges
                .To.PostgresqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => !IsReRunnableMigration(s))
                .JournalToPostgresqlTable("public", "migration_history")
                .LogToAutodetectedLog()
                .LogToConsole()
                .Build();

            var seeder = DeployChanges.To
                .PostgresqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), IsReRunnableMigration)
                .JournalTo(new NullJournal())
                .LogToAutodetectedLog()
                .Build();

            var sw = Stopwatch.StartNew();
            var upgradeResult = upgrader.PerformUpgrade();
            sw.Stop();
            _log.Information("Database schema migrated in {Elapsed}ms", sw.ElapsedMilliseconds);

            if (!upgradeResult.Successful)
            {
                throw new DbUpMigrationException("Failed to migrate database", upgradeResult);
            }

            sw.Restart();
            var seedResult = seeder.PerformUpgrade();
            sw.Stop();
            _log.Information("Database seeded in {Elapsed}ms", sw.ElapsedMilliseconds);

            if (!seedResult.Successful)
            {
                throw new DbUpMigrationException("Failed to seed database", seedResult);
            }

            Log.Logger.Information("Database migrated");
        }
    }

    public class DbUpMigrationException : Exception
    {
        public DbUpMigrationException(string message, DatabaseUpgradeResult result) : base(
            message + ": " + result.ErrorScript?.Name, result.Error)
        {
            ErrorScript = result.ErrorScript;
        }

        public SqlScript ErrorScript { get; }
    }
}