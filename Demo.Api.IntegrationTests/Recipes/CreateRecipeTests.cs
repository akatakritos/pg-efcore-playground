using System;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Demo.Api.Recipes;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Demo.Api.IntegrationTests.Recipes
{
    public class CreateRecipeTests: BaseIntegrationTest
    {
        [Fact]
        public async Task ARecipeIsCreated()
        {
            var cmd = new CreateRecipeCommand()
            {
                Name = "Chocolate Cake",
                Description = "Best birthday cake",
                PrepTime = Duration.FromMinutes(15),
                CookTime = Duration.FromHours(1)
            };

            var result = await AppFixture.SendAsync(cmd);
            result.Key.Should().NotBe(Guid.Empty);
            result.Version.Should().Be(1);

            var saved = await AppFixture.FindAsync<Recipe>(result.Key);
            saved.Name.Should().Be("Chocolate Cake");
            saved.Description.Should().Be("Best birthday cake");
            saved.CookTime.Should().Be(Duration.FromMinutes(60));
        }

    }
}
