using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Demo.Api.Customers
{
    public enum CustomerOrderBy
    {
        Name = 1,
        CreatedAt,
        UpdatedAt
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public class SearchCustomersRequest : IRequest<SearchCustomersResponse>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public CustomerOrderBy OrderBy { get; set; } = CustomerOrderBy.Name;
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
        public string NameContains { get; set; }
        public Instant? CreatedAfter { get; set; }
        public Instant? CreatedBefore { get; set; }
        public Instant? UpdatedBefore { get; set; }
        public Instant? UpdatedAfter { get; set; }
    }

    public class SearchResults<T>
    {
        public int TotalResults { get; set; }
        public IList<T> Results { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }

        public bool MoreResults => Offset + Results.Count <= TotalResults;
    }

    public class SearchCustomersResponse: SearchResults<GetCustomerResponse>
    {
    }

    public class SearchCustomersRequestValidator : AbstractValidator<SearchCustomersRequest>
    {
        public SearchCustomersRequestValidator()
        {
            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Limit).GreaterThan(0).LessThan(200);
            When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue, () =>
            {
                RuleFor(x => x.CreatedAfter)
                    .GreaterThan(x => x.CreatedBefore)
                    .WithMessage("CreatedAfter should be after CreatedBefore");
            });
            When(x => x.UpdatedAfter.HasValue && x.UpdatedBefore.HasValue, () =>
            {
                RuleFor(x => x.UpdatedAfter)
                    .GreaterThan(x => x.UpdatedBefore)
                    .WithMessage("UpdatedAfter should be greater than UpdatedBefore");
            });
        }
    }

    public class SearchCustomersHandler : IRequestHandler<SearchCustomersRequest, SearchCustomersResponse>
    {
        private readonly PlaygroundContext _context;
        private readonly IMapper _mapper;

        public SearchCustomersHandler(PlaygroundContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<SearchCustomersResponse> Handle(SearchCustomersRequest request,
                                                          CancellationToken cancellationToken)
        {
            IQueryable<Customer> query = _context.Customers;

            if (!string.IsNullOrEmpty(request.NameContains))
            {
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{request.NameContains}%"));
            }

            if (request.CreatedAfter.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.CreatedAfter);
            }

            if (request.CreatedBefore.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.CreatedBefore);
            }

            if (request.UpdatedBefore.HasValue)
            {
                query = query.Where(x => x.UpdatedAt <= request.UpdatedBefore);
            }

            if (request.UpdatedAfter.HasValue)
            {
                query = query.Where(x => x.UpdatedAt >= request.UpdatedAfter);
            }

            var count = await query.CountAsync(cancellationToken);

            query = request.OrderBy switch
            {
                CustomerOrderBy.Name => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(x => x.Name)
                    : query.OrderByDescending(x => x.Name),
                CustomerOrderBy.CreatedAt => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(x => x.CreatedAt)
                    : query.OrderByDescending(x => x.CreatedAt),
                CustomerOrderBy.UpdatedAt => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(x => x.UpdatedAt)
                    : query.OrderByDescending(x => x.UpdatedAt),
                _ => query
            };

            var results = await query
                .Skip(request.Offset)
                .Take(request.Limit)
                .ProjectTo<GetCustomerResponse>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

            return new SearchCustomersResponse()
            {
                Limit = request.Limit,
                Offset = request.Offset,
                Results = results,
                TotalResults = count
            };
        }
    }
}
