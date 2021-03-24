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

        // xunit runs test classes in parallel, so multiple
        // test classes could all try to hit this method at roughly the same time.
        // thus we need tou se a mutex to only allow one through at a time. The first
        // one through will go ahead and migrate the schema, then reset the tables
        // back to empty
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
