namespace OrderManagement.Domain.Common;

/// <summary>
/// An Aggregate Root is the single entry point for reading and modifying an aggregate.
/// It is responsible for enforcing the aggregate's invariants (business rules that must
/// always hold true) and for recording domain events as things happen inside it.
/// External code must never reach into an aggregate's internals directly -- everything
/// goes through the root's methods.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Called by infrastructure (e.g. after SaveChanges + dispatching events) to clear
    /// the events once they've been published, so they aren't raised twice.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
