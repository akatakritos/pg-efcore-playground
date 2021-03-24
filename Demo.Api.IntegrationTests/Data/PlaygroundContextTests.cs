using System.Threading.Tasks;
using Demo.Api.Data;
using Microsoft.EntityFrameworkCore;
using NFluent;
using Xunit;

namespace Demo.Api.IntegrationTests.Data
{
    public class PlaygroundContextTests: BaseIntegrationTest
    {
        [Fact]
        public async Task ItBumpsTheVersionAndEditTime()
        {
            var original = new Customer()
            {
                Name = "Bob"
            };

            await AppFixture.InsertAsync(original);

            await AppFixture.ExecuteDbContextAsync(async (db) =>
            {
                var saved = await db.Customers.FirstOrDefaultAsync(x => x.Key == original.Key);
                saved.Name = "Sally";
            });

            var updated = await AppFixture.FindAsync<Customer>(original.Key);

            Check.That(updated.Name).IsEqualTo("Sally");

            Check.That(updated.Version).IsEqualTo(2);

            Check.WithCustomMessage("UpdatedAt should be updated")
                .That(updated.UpdatedAt.ToUnixTimeTicks())
                .IsStrictlyGreaterThan(original.UpdatedAt.ToUnixTimeTicks());
            Check.WithCustomMessage("CreatedAt should not change")
                .That(updated.CreatedAt).IsEqualTo(updated.CreatedAt);
        }

        [Fact]
        public async Task ItTreatsVersionAsConcurrencyToken()
        {
            var original = new Customer()
            {
                Name = "Concurrency Check"
            };
            await AppFixture.InsertAsync(original);

            Check.ThatAsyncCode(async () =>
            {
                await AppFixture.ExecuteDbContextAsync(async (db) =>
                {
                    var saved = await db.Customers.FirstOrDefaultAsync(x => x.Key == original.Key);
                    saved.Name = "Sally";

                    // record is updated by another user while this context is working with the entity
                    // so when it goes to save, the version numbers will no longer match
                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $"update customers set version = version + 1 where key = {original.Key}");
                });
            }).Throws<DbUpdateConcurrencyException>();
        }

        [Fact]
        public async Task ItSoftDeletesRecords()
        {
            var original = new Customer() { Name = "Soft Delete Check" };
            await AppFixture.InsertAsync(original);

            await AppFixture.ExecuteDbContextAsync(async (db) =>
            {
                var saved = await db.Customers.FindAsync(original.Id);
                Check.WithCustomMessage("saved customer should exist").That(saved).IsNotNull();

                db.Remove(saved);
            });

            var deleted = await AppFixture.FindAsync<Customer>(original.Key);
            Check.WithCustomMessage($"Customer {original.Key} should be deleted").That(deleted).IsNull();

        }

    }
}
