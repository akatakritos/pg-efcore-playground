using System;
using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using Serilog;

namespace Demo.Api.Data.Migrations
{
    public class DbUpMigrator
    {
        private static ILogger _log = Log.ForContext<DbUpMigrator>();

        private readonly string _connectionString;

        public DbUpMigrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Migrate()
        {

            bool IsRerunnableMigration(string filename)
            {
                return filename.StartsWith("Demo.Api.Data.Migrations.R");
            }

            _log.Information("Migrating database");
            var upgrader = DeployChanges
                .To.PostgresqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => !IsRerunnableMigration(s))
                .JournalToPostgresqlTable("public", "migration_history")
                .LogToAutodetectedLog()
                .Build();

            var seeder = DeployChanges.To
                .PostgresqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), IsRerunnableMigration)
                .JournalTo(new NullJournal())
                .LogToAutodetectedLog()
                .Build();

            var upgradeResult = upgrader.PerformUpgrade();
            if (!upgradeResult.Successful)
            {
                throw new DbUpMigrationException("Failed to migrate database", upgradeResult);
            }

            var seedResult = seeder.PerformUpgrade();
            if (!seedResult.Successful)
            {
                throw new DbUpMigrationException("Failed to seed database", seedResult);
            }

            Log.Logger.Information("Database migrated");
        }
    }

    public class DbUpMigrationException : Exception
    {
        public SqlScript ErrorScript { get; }

        public DbUpMigrationException(string message, DatabaseUpgradeResult result) : base(message + ": " + result.ErrorScript?.Name, result.Error)
        {
            ErrorScript = result.ErrorScript;
        }
    }
}
