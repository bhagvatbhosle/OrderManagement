using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Orders.Events;

public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
