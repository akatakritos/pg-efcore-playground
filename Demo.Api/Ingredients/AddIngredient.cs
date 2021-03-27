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
    public class AddIngredientRequest : IRequest<ModelKey>
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

    public class AddIngredientHandler : IRequestHandler<AddIngredientRequest, ModelKey>
    {
        private readonly IMapper _mapper;
        private readonly PlaygroundContext _context;

        public AddIngredientHandler(IMapper mapper, PlaygroundContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ModelKey> Handle(AddIngredientRequest request, CancellationToken cancellationToken)
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

            var ingredient = existingIngredient ?? new Ingredient { Name = request.Name };
            recipe.AddIngredient(ingredient, request.UnitOfMeasure, request.Quantity);

            await _context.SaveChangesAsync(cancellationToken);

            var recipeIngredient = recipe.RecipeIngredients[^1];
            return new ModelKey() { Key = recipeIngredient.Key, Version = recipeIngredient.Version };
        }
    }

}
