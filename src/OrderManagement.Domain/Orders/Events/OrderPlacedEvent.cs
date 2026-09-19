using OrderManagement.Domain.Common;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Orders.Events;

/// <summary>
/// Raised when an Order transitions from Draft to Placed. Other parts of the system
/// (e.g. an Inventory bounded context, or a notification service) can subscribe to
/// this to react -- without the Order aggregate needing to know they exist.
/// </summary>
public sealed record OrderPlacedEvent(Guid OrderId, Guid CustomerId, Money Total) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
