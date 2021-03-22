using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Shared;
using FluentValidation;
using MediatR;

namespace Demo.Api.Customers
{
    public class CreateCustomerRequest : IRequest<ModelKey>
    {
        public string Name { get; set; }
    }

    public class CreateCustomerValidations : AbstractValidator<CreateCustomerRequest>
    {
        public CreateCustomerValidations()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class CreateCustomerRequestHandler : IRequestHandler<CreateCustomerRequest, ModelKey>
    {
        private readonly PlaygroundContext _context;
        private readonly IMapper _mapper;

        public CreateCustomerRequestHandler(PlaygroundContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ModelKey> Handle(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = _mapper.Map<Customer>(request);
            _context.Add((object) customer);
            await _context.SaveChangesAsync();
            return new ModelKey {Key = customer.Key, Version = customer.Version};
        }
    }
}
