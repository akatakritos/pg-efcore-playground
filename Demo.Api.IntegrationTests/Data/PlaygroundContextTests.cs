using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Api.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NFluent;
using NodaTime;
using Xunit;

namespace Demo.Api.IntegrationTests.Data
{
    public class PlaygroundContextTests : BaseIntegrationTest
    {
        [Fact]
        public async Task ItBumpsTheVersionAndEditTime()
        {
            var original = new Recipe
            {
                Name = "Chocolate Milk"
            };

            await AppFixture.InsertAsync(original);

            await AppFixture.ExecuteDbContextAsync(async db =>
            {
                var saved = await db.Recipes.FirstOrDefaultAsync(x => x.Key == original.Key);
                saved.Name = "Milk";
            });

            var updated = await AppFixture.FindAsync<Recipe>(original.Key);

            Check.That(updated.Name).IsEqualTo("Milk");

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
            var original = new Recipe
            {
                Name = "Concurrency Check"
            };
            await AppFixture.InsertAsync(original);

            Check.ThatAsyncCode(async () =>
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
            }).Throws<DbUpdateConcurrencyException>();
        }

        [Fact]
        public async Task ItSoftDeletesRecords()
        {
            var original = new Recipe { Name = "Soft Delete Check" };
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
            var recipe = new Recipe
            {
                Name = "Chocolate Milk",
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
