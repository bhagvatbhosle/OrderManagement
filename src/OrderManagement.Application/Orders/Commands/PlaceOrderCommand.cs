using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders.Commands;

public sealed record PlaceOrderCommand(Guid OrderId);

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public PlaceOrderCommandHandler(IOrderRepository orderRepository, IDomainEventDispatcher eventDispatcher)
    {
        _orderRepository = orderRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {command.OrderId} was not found.");

        order.Place(); // throws DomainException if invariants aren't met

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        // Dispatch events only AFTER the state change has been committed, so a listener
        // reacting to OrderPlacedEvent (e.g. reserving stock) never sees a "placed" order
        // that isn't actually saved yet.
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return OrderDto.FromDomain(order);
    }
}
