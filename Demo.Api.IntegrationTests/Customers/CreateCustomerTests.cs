using System.Threading.Tasks;
using Demo.Api.Customers;
using Demo.Api.Data;
using FluentValidation;
using NFluent;
using Xunit;

namespace Demo.Api.IntegrationTests.Customers
{
    public class CreateCustomerTests: BaseIntegrationTest
    {
        [Fact]
        public async Task CustomerNameRequired()
        {
            var request = new CreateCustomerRequest()
            {
                Name = ""
            };

            Check.ThatAsyncCode(async () =>
            {
                await AppFixture.SendAsync(request);
            }).Throws<ValidationException>();
        }

        [Fact]
        public async Task CustomerGetsCreated()
        {
            var request = new CreateCustomerRequest() { Name = "Create Test" };
            var result = await AppFixture.SendAsync(request);

            var saved = await AppFixture.FindAsync<Customer>(result.Key);
            Check.That(saved.Name).IsEqualTo("Create Test");
        }
    }
}
