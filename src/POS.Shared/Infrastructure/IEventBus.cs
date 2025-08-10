using POS.Shared.Domain.Events;

namespace POS.Shared.Infrastructure;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
    Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
}