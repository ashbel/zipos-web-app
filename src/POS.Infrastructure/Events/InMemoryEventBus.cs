using MediatR;
using POS.Shared.Domain.Events;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly IMediator _mediator;

    public InMemoryEventBus(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        await _mediator.Publish(@event, cancellationToken);
    }

    public async Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        await _mediator.Publish(@event, cancellationToken);
    }
}