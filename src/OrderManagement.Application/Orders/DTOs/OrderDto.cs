using OrderManagement.Domain.Orders;

namespace OrderManagement.Application.Orders.DTOs;

/// <summary>
/// Data Transfer Object: a flat, serialization-friendly shape for handing order data
/// across the Application/Api boundary. Never leak Domain entities/aggregates directly
/// out through an API -- that couples your HTTP contract to your domain model and makes
/// both harder to change independently.
/// </summary>
public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string Currency,
    decimal Total,
    string? ShippingAddress,
    DateTime CreatedOnUtc,
    DateTime? PlacedOnUtc,
    DateTime? ShippedOnUtc,
    IReadOnlyList<OrderLineDto> Lines)
{
    public static OrderDto FromDomain(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.Currency,
        order.Total.Amount,
        order.ShippingAddress?.ToString(),
        order.CreatedOnUtc,
        order.PlacedOnUtc,
        order.ShippedOnUtc,
        order.Lines.Select(OrderLineDto.FromDomain).ToList());
}

public sealed record OrderLineDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal)
{
    public static OrderLineDto FromDomain(OrderLine line) => new(
        line.ProductId,
        line.ProductName,
        line.Quantity,
        line.UnitPrice.Amount,
        line.LineTotal.Amount);
}
