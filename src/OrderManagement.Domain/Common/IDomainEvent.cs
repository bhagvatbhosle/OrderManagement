namespace OrderManagement.Domain.Common;

/// <summary>
/// Marker interface for something meaningful that happened in the domain.
/// Domain events are raised by aggregates and dispatched (e.g. via MediatR)
/// after the change has been persisted, so other parts of the system can react.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
