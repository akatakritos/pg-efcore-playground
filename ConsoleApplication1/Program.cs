using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Api.Data;
using Demo.Api.Domain;
using Faker;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Seed(10_000);
            return;

            // context.Customers.Add(customer);
            // await context.SaveChangesAsync();
            await Task.Delay(1000);
        }

        private static async Task Seed(int n = 1000)
        {
            for (var i = 0; i < n; i++)
            {
                var context = new PlaygroundContext(new DbContextOptionsBuilder<PlaygroundContext>()
                    .UseNpgsql("Host=localhost;Database=playground;Username=postgres;Password=LocalDev123",
                        o => o.UseNodaTime())
                    .UseSnakeCaseNamingConvention()
                    .EnableSensitiveDataLogging().Options);

                var recipe = new Recipe();
                recipe.Name = Faker.Company.Name();
                recipe.Description = Faker.Company.BS();
                recipe.CookTime = Duration.FromMinutes(Faker.RandomNumber.Next(1, 60));
                recipe.PrepTime = Duration.FromMinutes(Faker.RandomNumber.Next(1, 60));

                var ingredientCount = Faker.RandomNumber.Next(3, 8);
                for (int j = 0; j < ingredientCount; j++)
                {
                    var ingredientName = Faker.Name.FullName();
                    var existing = await context.Ingredients.FirstOrDefaultAsync(i => i.Name == ingredientName);
                    var ingredient = existing ?? new Ingredient() { Name = ingredientName };

                    var units = Faker.Enum.Random<UnitOfMeasure>();
                    var quantity = Faker.RandomNumber.Next(1, 16) / 4.0M;
                    recipe.AddIngredient(ingredient, units, quantity);
                }


                Console.WriteLine($"Entering recipe {i} of {n}");
                context.Recipes.Add(recipe);
                await context.SaveChangesAsync();
            }
        }
    }
}
