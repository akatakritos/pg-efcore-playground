using System.Threading.Tasks;
using Demo.Api.Data.Migrations;
using Nito.AsyncEx;
using Xunit;

namespace Demo.Api.IntegrationTests
{
    public class BaseIntegrationTest : IAsyncLifetime
    {
        private static readonly AsyncLock Mutex = new AsyncLock();

        private static bool _initialized;

        public virtual async Task InitializeAsync()
        {
            if (_initialized)
                return;

            using (await Mutex.LockAsync())
            {
                if (_initialized)
                    return;

                var migrator = new DbUpMigrator(AppFixture.ConnectionString);
                migrator.Migrate();

                await AppFixture.ResetCheckpoint();

                _initialized = true;
            }
        }

        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}
