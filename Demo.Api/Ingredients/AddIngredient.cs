using System;
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
        internal Guid RecipeKey { get; set; }

        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }
    }

    public class AddIngredientRequestValidator : AbstractValidator<AddIngredientRequest>
    {
        public AddIngredientRequestValidator()
        {
            RuleFor(x => x.RecipeKey).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Quantity).GreaterThan(0M);
            RuleFor(x => x.UnitOfMeasure).IsInEnum();
        }
    }

    public class AddIngredientHandler : IRequestHandler<AddIngredientRequest, ModelUpdateIdentifier>
    {
        private readonly IMapper _mapper;
        private readonly PlaygroundContext _context;

        public AddIngredientHandler(IMapper mapper, PlaygroundContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ModelUpdateIdentifier> Handle(AddIngredientRequest request, CancellationToken cancellationToken)
        {
            var recipe = await _context.Recipes
                .Include(x => x.RecipeIngredients)
                .ThenInclude(x => x.Ingredient)
                .Where(x => x.Key == request.RecipeKey)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (recipe == null) throw new RecordNotFoundException(nameof(Recipe), request.RecipeKey);

            var existingIngredient = await _context.Ingredients
                .Where(x => x.Name == request.Name)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var ingredient = existingIngredient ?? _mapper.Map<Ingredient>(request);
            var recipeIngredient = recipe.AddIngredient(ingredient, request.UnitOfMeasure, request.Quantity);

            await _context.SaveChangesAsync(cancellationToken);
            return new ModelUpdateIdentifier(recipeIngredient.Key, recipeIngredient.Version);
        }
    }

}
