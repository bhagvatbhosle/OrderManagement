using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Orders;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Orders.Commands;

public sealed record SetShippingAddressCommand(
    Guid OrderId,
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed class SetShippingAddressCommandHandler : ICommandHandler<SetShippingAddressCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public SetShippingAddressCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(SetShippingAddressCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {command.OrderId} was not found.");

        var address = Address.Create(
            command.Line1, command.Line2, command.City, command.State, command.PostalCode, command.Country);

        order.SetShippingAddress(address);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return OrderDto.FromDomain(order);
    }
}
