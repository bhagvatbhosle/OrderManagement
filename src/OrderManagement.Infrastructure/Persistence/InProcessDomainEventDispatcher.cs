using Microsoft.Extensions.Logging;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Common;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// The simplest possible domain event dispatcher: it just logs each event. This is
/// intentionally a stand-in -- in a real project you'd swap this out for MediatR
/// (publish each event as an INotification and let handlers subscribe) or a message
/// bus (publish to RabbitMQ/Azure Service Bus for other services to consume). Because
/// IDomainEventDispatcher is an interface the Application layer depends on, swapping
/// the implementation here never requires touching a single use case.
/// </summary>
public sealed class InProcessDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<InProcessDomainEventDispatcher> _logger;

    public InProcessDomainEventDispatcher(ILogger<InProcessDomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation(
                "Domain event dispatched: {EventType} at {OccurredOnUtc} -- {Event}",
                domainEvent.GetType().Name,
                domainEvent.OccurredOnUtc,
                domainEvent);
        }

        return Task.CompletedTask;
    }
}
