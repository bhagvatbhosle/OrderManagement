namespace OrderManagement.Domain.Orders;

/// <summary>
/// Gives the Application layer the illusion of an in-memory collection of Order
/// aggregates, hiding *how* they're actually persisted. The Domain defines this
/// interface; Infrastructure provides the real (e.g. EF Core) implementation.
/// This is what lets the Domain and Application layers be unit-tested with an
/// in-memory fake, no database required.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made to an already-tracked aggregate. With EF Core this is
    /// often a no-op (the context already tracks the entity) but keeping it explicit
    /// makes the repository's contract clear and keeps other implementations honest.
    /// </summary>
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
