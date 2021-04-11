using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Data;
using Demo.Api.Domain;
using MediatR;
using Serilog;

namespace Demo.Api.Infrastructure.Indexing
{
    public class IndexRecipeChangedHandler: INotificationHandler<RecipeChangedEvent>
    {
        private readonly RecipeIndexer _recipeIndexer;
        private readonly PlaygroundContext _context;
        private static readonly ILogger _log = Log.ForContext<IndexRecipeChangedHandler>();

        public IndexRecipeChangedHandler(RecipeIndexer recipeIndexer, PlaygroundContext context)
        {
            _recipeIndexer = recipeIndexer;
            _context = context;
        }

        public async Task Handle(RecipeChangedEvent notification, CancellationToken cancellationToken)
        {
            _log.Information("Indexing Recipe {Key}", notification.Key);
            var recipe = await _context.GetRecipe(notification.Key, cancellationToken);
            _recipeIndexer.IndexRecipe(recipe);
            _log.Information("Finished indexing Recipe {Key}", notification.Key);
        }
    }

}
