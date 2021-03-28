using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Domain;
using Demo.Api.Shared;
using FluentValidation;
using MediatR;
using NodaTime;

namespace Demo.Api.Recipes
{
    public class CreateRecipeCommand: IRequest<ModelUpdateIdentifier>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Duration CookTime { get; set; }
        public Duration PrepTime { get; set; }
    }

    public class CreateRecipeValidator : AbstractValidator<CreateRecipeCommand>
    {
        public CreateRecipeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .NotNull()
                .MaximumLength(256);
        }
    }

    public class CreateRecipeCommandHandler : IRequestHandler<CreateRecipeCommand, ModelUpdateIdentifier>
    {
        private readonly IMapper _mapper;
        private readonly PlaygroundContext _context;

        public CreateRecipeCommandHandler(IMapper mapper, PlaygroundContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ModelUpdateIdentifier> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
        {
            var recipe = _mapper.Map<Recipe>(request);
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync(cancellationToken);
            return new ModelUpdateIdentifier(recipe);
        }
    }
}
