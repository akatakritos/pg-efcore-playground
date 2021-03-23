using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Serilog;
using Serilog.Core;
using StackExchange.Profiling;

namespace Demo.Api.Infrastructure
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        // Log.ForContext<Type> gives a gnarly name due to generics
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _log = Log.ForContext(Constants.SourceContextPropertyName,
            typeof(ValidationBehavior<,>).FullName);

        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            using (MiniProfiler.Current.Step("Checking Validations"))
            {
                if (_validators.Any())
                {
                    var context = new ValidationContext<TRequest>(request);
                    var validationResults =
                        await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                    var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
                    if (failures.Count != 0)
                    {
                        _log.Debug("Request {RequestName} failed validation with {ErrorCount} errors",
                            typeof(TRequest).Name, failures.Count);
                        throw new ValidationException(failures);
                    }
                }
            }

            _log.Debug("Request {HandlerName} passed validation", typeof(TRequest).Name);
            return await next();
        }
    }
}
