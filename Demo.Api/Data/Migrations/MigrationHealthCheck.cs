using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Demo.Api.Data;
using Demo.Api.Data.Migrations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Demo.Api.Infrastructure
{
    public class MigrationHealthCheck : IHealthCheck
    {
        private readonly IDatabase _db;
        private readonly IList<string> _migrationScripts;

        public MigrationHealthCheck(IDatabase db)
        {
            _db = db;
            var assembly = Assembly.GetAssembly(typeof(DbUpMigrator));
            _migrationScripts = assembly.GetManifestResourceNames()
                .Where(s => !DbUpMigrator.IsReRunnableMigration(s))
                .ToList();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
                                                              CancellationToken cancellationToken = default)
        {
            using var connection = await _db.GetOpenConnection(cancellationToken);
            var appliedScripts =
                (await connection.QueryAsync<MigrationHistory>(@"select ""scriptname"" from ""migration_history"""))
                .Select(s => s.ScriptName)
                .ToList();

            var unapplied = _migrationScripts.Except(appliedScripts).ToList();
            if (unapplied.Count > 0)
            {
                var extraData = new Dictionary<string, object>
                {
                    { "unAppliedScripts", unapplied }
                };

                return HealthCheckResult.Degraded("Found embedded migrations that have not been run", data: extraData);
            }

            return HealthCheckResult.Healthy("All embedded migrations have been executed",
                new Dictionary<string, object>
                {
                    { "migrations", appliedScripts }
                });
        }

        private class MigrationHistory
        {
            public string ScriptName { get; set; }
        }
    }
}