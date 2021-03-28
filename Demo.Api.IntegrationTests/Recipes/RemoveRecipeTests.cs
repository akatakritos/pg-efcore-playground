using System;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Demo.Api.Recipes;
using Demo.Api.Shared;
using FluentAssertions;
using Xunit;

namespace Demo.Api.IntegrationTests.Recipes
{
    public class RemoveRecipeTests : BaseIntegrationTest
    {
        [Fact]
        public async Task ItDeletesRecipeAndIngredients()
        {
            var recipe = new Recipe() { Name = nameof(ItDeletesRecipeAndIngredients) };
            var ingredient = new Ingredient() { Name = Guid.NewGuid().ToString() };
            recipe.AddIngredient(ingredient, UnitOfMeasure.Cup, 10M);
            await AppFixture.InsertAsync(recipe);

            await AppFixture.SendAsync(new RemoveRecipeRequest()
            {
                RecipeKey = new ModelUpdateIdentifier(recipe)
            });

            var deleted = await AppFixture.ExecuteDbContextAsync(db => db.GetRecipe(recipe.Key));
            deleted.Should().BeNull(because: "it was soft-deleted");

            var deletedIngredient = await AppFixture.FindAsync<RecipeIngredient>(ingredient.Key);
            deletedIngredient.Should().BeNull(because: "it was soft-deleted");

        }

    }
}
