using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Demo.Api.Ingredients;
using Demo.Api.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Demo.Api.IntegrationTests.Ingredients
{
    public class RemoveIngredientTests : BaseIntegrationTest
    {
        [Fact]
        public async Task ItRemovesTheIngredient()
        {
            var original = new Recipe(name: nameof(ItRemovesTheIngredient));

            var toDelete = original.AddIngredient(new Ingredient(Guid.NewGuid().ToString()),
                UnitOfMeasure.Cup, 2M);
            original.AddIngredient(new Ingredient(Guid.NewGuid().ToString()), UnitOfMeasure.Pint, 5M);

            await AppFixture.InsertAsync(original);

            var request = new RemoveIngredientRequest
            {
                RecipeModelKey = new ModelUpdateIdentifier(original),
                RecipeIngredientModelKey = new ModelUpdateIdentifier(toDelete)
            };

            await AppFixture.SendAsync(request);

            var saved = await AppFixture.ExecuteDbContextAsync(async db =>
            {
                return await db.Recipes.Where(x => x.Id == original.Id).Include(x => x.RecipeIngredients)
                    .FirstOrDefaultAsync();
            });

            saved.RecipeIngredients.Should().HaveCount(1);
            saved.RecipeIngredients[0].UnitOfMeasure.Should().Be(UnitOfMeasure.Pint);
        }
    }
}
