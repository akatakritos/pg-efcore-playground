using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Domain;
using Demo.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Api.Ingredients
{
    public class RemoveIngredientRequest : IRequest
    {
        public ModelUpdateIdentifier RecipeModelKey { get; set; }
        public ModelUpdateIdentifier RecipeIngredientModelKey { get; set; }
    }

    public class RemoveIngredientRequestHandler : IRequestHandler<RemoveIngredientRequest>
    {
        private readonly PlaygroundContext _context;
        private readonly IMapper _mapper;

        public RemoveIngredientRequestHandler(IMapper mapper, PlaygroundContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Unit> Handle(RemoveIngredientRequest request, CancellationToken cancellationToken)
        {
            var recipes = await _context.Recipes
                .Where(x => x.Key == request.RecipeModelKey.Key &&
                            x.Version == request.RecipeModelKey.Version)
                .Include(x => x.RecipeIngredients)
                .FirstOrDefaultAsync(cancellationToken);

            if (recipes == null)
            {
                throw new RecordNotFoundException(nameof(Recipe), request.RecipeModelKey);
            }

            recipes.RemoveIngredient(request.RecipeIngredientModelKey);

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}