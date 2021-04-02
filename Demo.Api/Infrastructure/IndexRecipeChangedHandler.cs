using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Domain;
using MediatR;
using Serilog;

namespace Demo.Api.Infrastructure
{
    public class IndexRecipeChangedHandler: INotificationHandler<RecipeChangedEvent>
    {
        private static readonly ILogger _log = Log.ForContext<IndexRecipeChangedHandler>();

        public async Task Handle(RecipeChangedEvent notification, CancellationToken cancellationToken)
        {
            _log.Information("Indexing Recipe {Key}", notification.Key);
            await Task.Delay(2000, cancellationToken);
            _log.Information("Finished indexing Recipe {Key}", notification.Key);
        }
    }
}
