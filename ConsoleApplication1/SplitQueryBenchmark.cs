using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Demo.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApplication1
{
    public class SplitQueryBenchmark
    {
        private DbContextOptions<PlaygroundContext> _dbOptions;
        private Guid[] _keys;
        private Random _rng = new Random();

        public SplitQueryBenchmark()
        {
            _dbOptions = new DbContextOptionsBuilder<PlaygroundContext>()
                .UseNpgsql("Host=localhost;Database=playground;Username=postgres;Password=LocalDev123",
                    o => o.UseNodaTime())
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging().Options;

            using var context = new PlaygroundContext(_dbOptions);
            _keys = context.Recipes.Select(r => r.Key).Take(200).ToArray();
        }

        private Guid RandomKey()
        {
            return _keys[_rng.Next(0, _keys.Length)];
        }

        [Benchmark]
        public void SplitQuery()
        {
            var key = RandomKey();
            using var context = new PlaygroundContext(_dbOptions);
            var recipe = context.Recipes
                .Include(r => r.RecipeIngredients)
                .AsSplitQuery()
                .First(r => r.Key == key);
        }

        [Benchmark]
        public void JoinedQuery()
        {
            var key = RandomKey();
            using var context = new PlaygroundContext(_dbOptions);
            var recipe = context.Recipes
                .Include(r => r.RecipeIngredients)
                .First(r => r.Key == key);
        }
    }
}
