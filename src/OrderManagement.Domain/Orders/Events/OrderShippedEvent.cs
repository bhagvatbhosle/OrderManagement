using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Orders.Events;

public sealed record OrderShippedEvent(Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
