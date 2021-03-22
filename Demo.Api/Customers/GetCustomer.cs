using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Api.Data;
using Demo.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Demo.Api.Customers
{
    public class GetCustomerResponse : IModelKeyed
    {
        public string Name { get; set; }
        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
        public ModelKey ModelKey { get; set; }
    }

    public class GetCustomerRequest : IRequest<GetCustomerResponse>
    {
        public Guid Key { get; set; }
    }

    public class GetCustomerRequestHandler : IRequestHandler<GetCustomerRequest, GetCustomerResponse>
    {
        private readonly PlaygroundContext _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetCustomerRequestHandler(PlaygroundContext context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public async Task<GetCustomerResponse> Handle(GetCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .Where(c => c.Key == request.Key)
                .ProjectTo<GetCustomerResponse>(_mapperConfig)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                throw new RecordNotFoundException(nameof(Customer), request.Key);
            }

            return customer;
        }
    }
}