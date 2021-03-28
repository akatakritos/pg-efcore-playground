using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Api.Data;
using Demo.Api.Domain;
using Demo.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Demo.Api.Recipes
{
    public class ModelResponseBase
    {
        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
        public ModelUpdateIdentifier ModelKey { get; set; }
    }

    public class RecipeResponse : ModelResponseBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Duration CookTime { get; set; }
        public Duration PrepTime { get; set; }
        public IReadOnlyList<RecipeIngredientResponse> RecipeIngredients { get; set; }
    }

    public class RecipeIngredientResponse : ModelResponseBase
    {
        public UnitOfMeasure UnitOfMeasure { get; set; }
        public IngredientResponse Ingredient { get; set; }
        public decimal Quantity { get; set; }
    }

    public class IngredientResponse : ModelResponseBase
    {
        public string Name { get; set; }
    }

    public class GetRecipeRequest : IRequest<RecipeResponse>
    {
        public Guid Key { get; set; }
    }

    public class GetRecipeRequestHandler : IRequestHandler<GetRecipeRequest, RecipeResponse>
    {
        private readonly PlaygroundContext _context;
        private readonly IMapper _mapper;

        public GetRecipeRequestHandler(PlaygroundContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RecipeResponse> Handle(GetRecipeRequest request, CancellationToken cancellationToken)
        {
            var recipe = await _context.Recipes
                .Where(r => r.Key == request.Key)
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .ProjectTo<RecipeResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (recipe == null)
            {
                throw new RecordNotFoundException(nameof(Recipe), request.Key);
            }

            return recipe;
        }
    }
}