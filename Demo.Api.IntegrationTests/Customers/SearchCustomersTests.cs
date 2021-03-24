using System;
using System.Threading.Tasks;
using Demo.Api.Customers;
using Demo.Api.Data;
using NFluent;
using Xunit;

namespace Demo.Api.IntegrationTests.Customers
{
    public class SearchCustomersTests : BaseIntegrationTest
    {
        [Fact]
        public async Task CanFindCustomersByName()
        {
            var s = Guid.NewGuid().ToString("N");
            var c1 = new Customer() { Name = s };
            var c2 = new Customer() { Name = $"suffxed {s}" };
            var c3 = new Customer() { Name = $"{s} prefixed" };
            var c4 = new Customer() { Name = $"in the middle {s} of it" };

            await AppFixture.InsertAsync(c1, c2, c3, c4);

            var results = await AppFixture.SendAsync(new SearchCustomersRequest()
            {
                NameContains = s,
                Offset = 0,
                Limit = 2
            });

            Check.WithCustomMessage("should be 4 total results").That(results.TotalResults).IsEqualTo(4);
            Check.WithCustomMessage("should be 2 results in the page").That(results.Results).HasSize(2);
            Check.WithCustomMessage("should be more results").That(results.MoreResults).IsTrue();
        }
    }
}
