using OrderManagement.Domain.Common;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Orders;

/// <summary>
/// A single line within an Order: "3 units of Product X at $9.99 each".
/// Modeled as a Value Object (not an Entity) because a line has no identity of its own --
/// it's fully described by its product, quantity and price, and it only ever exists
/// as part of an Order aggregate. If you needed to reference a specific line later
/// (e.g. for partial refunds), you'd promote it to an Entity with its own Id.
/// </summary>
public sealed class OrderLine : ValueObject
{
    public Guid ProductId { get; } = default!;
    public string ProductName { get; } = default!;
    public int Quantity { get; } = default!;
    public Money UnitPrice { get; } = default!;

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    // EF Core needs this to materialize an OrderLine: the 4-arg constructor below can't
    // be used for that purpose because UnitPrice is an owned navigation (Money), and EF
    // Core's constructor binding can only bind scalar properties through a constructor --
    // owned/related types must be set via field access after construction. With this
    // parameterless constructor present, EF falls back to it and populates every
    // property (including UnitPrice) directly through the backing fields instead.
    private OrderLine() { }

    private OrderLine(Guid productId, string productName, int quantity, Money unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static OrderLine Create(Guid productId, string productName, int quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Order line must reference a valid product.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Order line must have a product name.");

        if (quantity <= 0)
            throw new DomainException("Order line quantity must be greater than zero.");

        return new OrderLine(productId, productName.Trim(), quantity, unitPrice);
    }

    /// <summary>
    /// Value Objects are immutable, so "changing" the quantity means creating a new line.
    /// </summary>
    public OrderLine WithQuantity(int newQuantity) => Create(ProductId, ProductName, newQuantity, UnitPrice);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return Quantity;
        yield return UnitPrice;
    }
}
