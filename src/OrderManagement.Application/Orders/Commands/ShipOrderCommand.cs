using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders.Commands;

public sealed record ShipOrderCommand(Guid OrderId);

public sealed class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public ShipOrderCommandHandler(IOrderRepository orderRepository, IDomainEventDispatcher eventDispatcher)
    {
        _orderRepository = orderRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<OrderDto> Handle(ShipOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {command.OrderId} was not found.");

        order.Ship();

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return OrderDto.FromDomain(order);
    }
}
