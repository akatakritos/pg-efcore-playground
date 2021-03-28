using System;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Data;
using Demo.Api.Shared;
using MediatR;

namespace Demo.Api.Recipes
{
    public class RemoveRecipeRequest: IRequest<Unit>
    {
        public ModelUpdateIdentifier RecipeKey { get; set; }
    }

    public class RemoveRecipeRequestHandler : IRequestHandler<RemoveRecipeRequest, Unit>
    {
        private readonly PlaygroundContext _context;

        public RemoveRecipeRequestHandler(PlaygroundContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RemoveRecipeRequest request, CancellationToken cancellationToken)
        {
            var recipe = await _context.GetRecipeForUpdate(request.RecipeKey, cancellationToken);
            recipe.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
