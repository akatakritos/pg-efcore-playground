using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Data;
using Demo.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Demo.Api.Infrastructure.Indexing
{
    public class IndexAllRecipes
    {
        private readonly PlaygroundContext _context;
        private readonly RecipeIndexer _indexer;

        public IndexAllRecipes(PlaygroundContext context, RecipeIndexer indexer)
        {
            _context = context;
            _indexer = indexer;
        }

        public async Task IndexAll(CancellationToken cancellationToken = default)
        {
            const int batchSize = 100;
            var currentSkip = 0;
            List<Recipe> batch = null;

            do
            {
                batch = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                    .OrderBy(ri => ri.Id)
                    .Skip(currentSkip)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                _indexer.IndexRecipes(batch);
                currentSkip += batchSize;
                Console.WriteLine("Indexed " + currentSkip);
            } while (batch.Count > 0);
        }
    }
}