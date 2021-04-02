using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Demo.Api.Data;
using MediatR;

namespace Demo.Api.Infrastructure
{
    public class BackgroundMessageDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;
        private readonly Channel<IDomainEvent> _channel = Channel.CreateUnbounded<IDomainEvent>();

        public BackgroundMessageDispatcher(IMediator mediator)
        {
            _mediator = mediator;
            Task.Run(ChannelReader);
        }

        private async Task ChannelReader()
        {
            await foreach (var @event in _channel.Reader.ReadAllAsync())
            {
                await _mediator.Publish(@event);
            }
        }

        public async Task DispatchAsync(IDomainEvent @event)
        {
            await _channel.Writer.WriteAsync(@event);
        }
    }
}
