using OrderManagement.Domain.Common;

namespace OrderManagement.Application.Abstractions;

/// <summary>
/// Publishes the domain events an aggregate collected while a use case ran. Kept as an
/// abstraction here so the Application layer doesn't depend on a specific messaging
/// library; Infrastructure provides the real implementation (in this starter, a simple
/// in-process dispatcher -- swap in MediatR or a message bus later without touching
/// any use case).
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
