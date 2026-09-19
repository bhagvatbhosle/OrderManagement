using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Orders;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Orders.Commands;

public sealed record AddOrderLineCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed class AddOrderLineCommandHandler : ICommandHandler<AddOrderLineCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public AddOrderLineCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(AddOrderLineCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {command.OrderId} was not found.");

        // The order enforces its own invariants (e.g. must be Draft, currency must match).
        order.AddLine(command.ProductId, command.ProductName, command.Quantity, Money.Of(command.UnitPrice, order.Currency));

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return OrderDto.FromDomain(order);
    }
}
