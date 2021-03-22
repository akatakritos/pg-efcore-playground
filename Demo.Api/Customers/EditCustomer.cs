using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Api.Customers
{
    public class EditCustomerRequest
    {
        public string Name { get; set; }
    }

    public class EditCustomerCommand : EditCustomerRequest, IRequest<ModelKey>
    {
        public ModelKey ModelKey { get; set; }
        public string Name { get; set; }
    }

    public class EditCustomerCommandValidator : AbstractValidator<EditCustomerCommand>
    {
        public EditCustomerCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class EditCustomerCommandHandler : IRequestHandler<EditCustomerCommand, ModelKey>
    {
        private readonly IMapper _mapper;
        private readonly PlaygroundContext _context;

        public EditCustomerCommandHandler(IMapper mapper, PlaygroundContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ModelKey> Handle(EditCustomerCommand request, CancellationToken cancellationToken)
        {
            var existing = await _context.Customers
                .Where(c => c.Key == request.ModelKey.Key && c.Version == request.ModelKey.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null) throw new RecordNotFoundException(nameof(Customer), request.ModelKey);

            _mapper.Map(request, existing);

            await _context.SaveChangesAsync(cancellationToken);
            return new ModelKey() {Key = existing.Key, Version = existing.Version};
        }
    }

}
