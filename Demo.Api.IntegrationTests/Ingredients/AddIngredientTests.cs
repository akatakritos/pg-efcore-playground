using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Api.Data;
using Demo.Api.Domain;
using Demo.Api.Ingredients;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Demo.Api.IntegrationTests.Ingredients
{
    public class AddIngredientTests: BaseIntegrationTest
    {
        [Fact]
        public async Task ItAddsAnIngredient()
        {
            var ingredientName = Guid.NewGuid().ToString();

            var recipe = new Recipe()
            {
                Name = nameof(ItAddsAnIngredient)
            };
            await AppFixture.InsertAsync(recipe);

            var result = await AppFixture.SendAsync(new AddIngredientRequest()
            {
                RecipeKey = recipe.Key,
                Name = ingredientName,
                Quantity = 42M,
                UnitOfMeasure = UnitOfMeasure.Pint
            });

            var saved = await AppFixture.ExecuteDbContextAsync(db => db.RecipeIngredients
                .Include(ri => ri.Ingredient)
                .Include(ri => ri.Recipe)
                .Where(ri => ri.Key == result.Key)
                .FirstOrDefaultAsync());

            saved.Ingredient.Name.Should().Be(ingredientName);
            saved.Recipe.Key.Should().Be(recipe.Key);
            saved.UnitOfMeasure.Should().Be(UnitOfMeasure.Pint);
            saved.Quantity.Should().Be(42M);
        }

    }
}
