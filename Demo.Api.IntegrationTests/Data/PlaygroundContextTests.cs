using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Api.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Demo.Api.IntegrationTests.Data
{
    public class PlaygroundContextTests : BaseIntegrationTest
    {
        [Fact]
        public async Task ItBumpsTheVersionAndEditTime()
        {
            var original = new Recipe(name: nameof(ItBumpsTheVersionAndEditTime));

            await AppFixture.InsertAsync(original);

            await AppFixture.ExecuteDbContextAsync(async db =>
            {
                var saved = await db.Recipes.FirstOrDefaultAsync(x => x.Key == original.Key);
                saved.Name = "Milk";
            });

            var updated = await AppFixture.FindAsync<Recipe>(original.Key);

            updated.Name.Should().Be("Milk");
            updated.Version.Should().Be(2);

            updated.UpdatedAt.ToUnixTimeMilliseconds().Should()
                .BeGreaterThan(original.UpdatedAt.ToUnixTimeMilliseconds(),
                    because: "updated timestamps should be bumped on change");
            updated.CreatedAt.Should().Be(original.CreatedAt, because: "created timestamps are never changed");
        }

        [Fact]
        public async Task ItTreatsVersionAsConcurrencyToken()
        {
            var original = new Recipe(name: nameof(ItTreatsVersionAsConcurrencyToken));
            await AppFixture.InsertAsync(original);

            FluentActions.Invoking(async () =>
            {
                await AppFixture.ExecuteDbContextAsync(async db =>
                {
                    var saved = await db.Recipes.FirstOrDefaultAsync(x => x.Key == original.Key);
                    saved.Name = "Sally";

                    // record is updated by another user while this context is working with the entity
                    // so when it goes to save, the version numbers will no longer match
                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $"update recipes set version = version + 1 where key = {original.Key}");
                });
            }).Should().Throw<DbUpdateConcurrencyException>();
        }

        [Fact]
        public async Task ItSoftDeletesRecords()
        {
            var original = new Recipe(name: nameof(ItSoftDeletesRecords));
            await AppFixture.InsertAsync(original);

            await AppFixture.ExecuteDbContextAsync(async db =>
            {
                var saved = await db.Recipes.FindAsync(original.Id);
                saved.Should().NotBeNull("we just saved it");

                saved.SoftDelete();
                // db.Remove(saved);
            });

            var deleted = await AppFixture.FindAsync<Recipe>(original.Key);
            deleted.Should().BeNull("Recipe {0} was soft-deleted", original.Key);
        }

        [Fact]
        public async Task CanSaveASimpleRecipe()
        {
            var recipe = new Recipe(name: "Chocolate Milk")
            {
                PrepTime = Duration.FromMinutes(3),
                CookTime = Duration.Zero
            };

            var milk = new Ingredient
            {
                Name = "Milk"
            };

            var chocolateSauce = new Ingredient
            {
                Name = "Chocolate Sauce"
            };

            recipe.AddIngredient(milk, UnitOfMeasure.Cup, 1M);
            recipe.AddIngredient(chocolateSauce, UnitOfMeasure.Tablespoon, 2M);

            await AppFixture.InsertAsync(recipe);

            var saved = await AppFixture.ExecuteDbContextAsync(db =>
                db.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                    .Where(r => r.Key == recipe.Key)
                    .FirstOrDefaultAsync());
            saved.Name.Should().Be("Chocolate Milk");
            saved.PrepTime.Should().Be(Duration.FromMinutes(3));
            saved.CookTime.Should().Be(Duration.Zero);
            saved.RecipeIngredients.Should().HaveCount(2)
                .And.SatisfyRespectively(
                    first =>
                    {
                        first.Id.Should().BeGreaterThan(0);
                        first.Key.Should().NotBe(Guid.Empty);
                        first.Quantity.Should().Be(1M);
                        first.UnitOfMeasure.Should().Be(UnitOfMeasure.Cup);
                        first.Ingredient.Should().Be(milk);
                    },
                    second =>
                    {
                        second.Id.Should().BeGreaterThan(0);
                        second.Key.Should().NotBe(Guid.Empty);
                        second.Quantity.Should().Be(2M);
                        second.UnitOfMeasure.Should().Be(UnitOfMeasure.Tablespoon);
                        second.Ingredient.Should().Be(chocolateSauce);
                    });
        }
    }
}
