using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders.Commands;

public sealed record CreateOrderCommand(Guid CustomerId, string Currency = "USD");

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        // All the validation (customer id must be valid, etc.) lives inside Order.Create --
        // the handler's job is only to orchestrate, never to duplicate business rules.
        var order = Order.Create(command.CustomerId, command.Currency);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return OrderDto.FromDomain(order);
    }
}
