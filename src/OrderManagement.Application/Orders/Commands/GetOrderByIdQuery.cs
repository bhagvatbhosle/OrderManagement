using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders.Commands;

/// <summary>
/// Not a "command" in the CQRS sense (it doesn't change state), but kept in the same
/// folder for this small starter project. In a larger app you'd split Commands and
/// Queries into separate folders/namespaces once the distinction starts pulling weight.
/// </summary>
public sealed record GetOrderByIdQuery(Guid OrderId);

public sealed class GetOrderByIdQueryHandler : ICommandHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {query.OrderId} was not found.");

        return OrderDto.FromDomain(order);
    }
}
