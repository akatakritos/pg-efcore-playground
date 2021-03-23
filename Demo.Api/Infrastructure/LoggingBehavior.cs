using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;
using Serilog.Core;

namespace Demo.Api.Infrastructure
{
    public class LoggingBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
        where TRequest: IRequest<TResponse>
    {
        // Log.ForContext<Type> gives a gnarly name due to generics
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger _log = Log.ForContext(Constants.SourceContextPropertyName,
            typeof(LoggingBehavior<,>).FullName);

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _log.Information("Running handler for {RequestName}", typeof(TRequest).Name);

            var sw = Stopwatch.StartNew();
            var result = await next();
            sw.Stop();

            _log.Information("Completed {RequestName} handler in {Duration}ms",
                typeof(TRequest).Name, sw.ElapsedMilliseconds);
            return result;
        }
    }
}
