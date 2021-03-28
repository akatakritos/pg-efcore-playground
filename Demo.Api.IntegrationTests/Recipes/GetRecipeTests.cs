using System;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Demo.Api.Recipes;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Demo.Api.IntegrationTests.Recipes
{
    public class GetRecipeTests
    {
        [Fact]
        public async Task GetRecipe_FetchesAndMaps()
        {
            var recipe = await AppFixture.ExecuteDbContextAsync(async db =>
            {
                var recipe = new Recipe(name: nameof(GetRecipe_FetchesAndMaps) + Guid.NewGuid())
                {
                    Description = "Testing a load",
                    CookTime = Duration.FromMinutes(60),
                    PrepTime = Duration.FromMinutes(15)
                };

                var ingredient = new Ingredient("Sugar");
                recipe.AddIngredient(ingredient, UnitOfMeasure.Cup, 1M);
                db.Recipes.Add(recipe);
                await Task.CompletedTask;
                return recipe;
            });

            var response = await AppFixture.SendAsync(new GetRecipeRequest { Key = recipe.Key });
            response.Name.Should().StartWith(nameof(GetRecipe_FetchesAndMaps));
            response.CookTime.Should().Be(Duration.FromHours(1));
            response.RecipeIngredients.Should().HaveCount(1);
            recipe.RecipeIngredients[0].Ingredient.Name.Should().Be("Sugar");
        }
    }
}
