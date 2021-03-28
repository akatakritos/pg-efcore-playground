using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Demo.Api.Data;
using Demo.Api.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Demo.Api.IntegrationTests.Data
{
    public class UnitOfMeasureTests
    {
        private record UnitOfMeasureRow(int Id, string Name);

        [Fact]
        public async Task AllValuesAreDefined()
        {
            var records = await AppFixture.ExecuteScopeAsync(async services =>
            {
                var db = services.GetService<IDatabase>();
                using var connection = await db.GetOpenConnection();
                return (await connection.QueryAsync<UnitOfMeasureRow>("select * from unit_of_measure_lib"))
                    .ToList();
            });

            foreach (var record in records)
            {
                var exists = UnitOfMeasure.TryFromValue(record.Id, out _);
                exists.Should().BeTrue(because: "we should set up the enum to match the database");
            }
        }
    }
}
