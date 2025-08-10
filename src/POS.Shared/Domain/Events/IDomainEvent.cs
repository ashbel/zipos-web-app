using MediatR;

namespace POS.Shared.Domain.Events;

public interface IDomainEvent : INotification
{
    string EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}