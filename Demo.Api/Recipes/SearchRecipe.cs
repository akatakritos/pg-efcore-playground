using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Api.Data;
using Demo.Api.Infrastructure.Indexing;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Api.Recipes
{
    public record SearchRecipeRequest(string SearchText, int Skip = 0, int Take = 25): IRequest<SearchRecipeResponse> {}

    public class SearchRecipeRequestValidator : AbstractValidator<SearchRecipeRequest>
    {
        public SearchRecipeRequestValidator()
        {
            RuleFor(x => x.SearchText).NotEmpty();
            RuleFor(x => x.Skip).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Take).LessThanOrEqualTo(100);
        }
    }

    public class SearchResult<T>
    {
        public IList<T> Items { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int TotalResults { get; set; }
    }

    public class SearchRecipeResponse: SearchResult<RecipeResponse>{}

    public class SearchRecipeHandler : IRequestHandler<SearchRecipeRequest, SearchRecipeResponse>
    {
        private readonly RecipeIndexSearcher _searcher;
        private readonly PlaygroundContext _context;
        private readonly IMapper _mapper;

        public SearchRecipeHandler(RecipeIndexSearcher searcher, PlaygroundContext context, IMapper _mapper)
        {
            _searcher = searcher;
            _context = context;
            this._mapper = _mapper;
        }

        public async Task<SearchRecipeResponse> Handle(SearchRecipeRequest request, CancellationToken cancellationToken)
        {
            var results = _searcher.Search(request.SearchText, request.Skip, request.Take);
            var keys = results.Results.Select(r => r.Key);
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => keys.Contains(r.Key))
                .ProjectTo<RecipeResponse>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new SearchRecipeResponse()
            {
                TotalResults = results.TotalHits,
                Items = recipes,
                Skip = request.Skip,
                Take = request.Take
            };
        }
    }

}
