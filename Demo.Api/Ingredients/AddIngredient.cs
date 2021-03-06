using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Domain;
using Demo.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Api.Ingredients
{
    public class AddIngredientRequest : IRequest<ModelUpdateIdentifier>
    {
        // comes from url, not meant to be POSTed, so internal
        internal ModelUpdateIdentifier RecipeKey { get; set; } = null!;

        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    }

    public class AddIngredientRequestValidator : AbstractValidator<AddIngredientRequest>
    {
        public AddIngredientRequestValidator()
        {
            RuleFor(x => x.RecipeKey).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Quantity).GreaterThan(0M);
            RuleFor(x => x.UnitOfMeasure).NotNull();
            RuleFor(x => x.RecipeKey).NotNull();
        }
    }

    public class AddIngredientHandler : IRequestHandler<AddIngredientRequest, ModelUpdateIdentifier>
    {
        private readonly PlaygroundContext _context;
        private readonly IMapper _mapper;

        public AddIngredientHandler(IMapper mapper, PlaygroundContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ModelUpdateIdentifier> Handle(AddIngredientRequest request,
                                                        CancellationToken cancellationToken)
        {
            var recipe = await _context.GetRecipeForUpdate(request.RecipeKey);

            if (recipe == null)
            {
                throw new RecordNotFoundException(nameof(Recipe), request.RecipeKey);
            }

            var existingIngredient = await _context.Ingredients
                .Where(x => x.Name == request.Name)
                .FirstOrDefaultAsync(cancellationToken);

            var ingredient = existingIngredient ?? _mapper.Map<Ingredient>(request);
            var recipeIngredient = recipe.AddIngredient(ingredient, request.UnitOfMeasure, request.Quantity);

            await _context.SaveChangesAsync(cancellationToken);
            return new ModelUpdateIdentifier(recipeIngredient.Key, recipeIngredient.Version);
        }
    }
}
