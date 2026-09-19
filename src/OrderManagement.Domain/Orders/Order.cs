using OrderManagement.Domain.Common;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Orders.Events;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Orders;

/// <summary>
/// Aggregate Root for the Order aggregate. This is the ONLY entry point for reading or
/// changing an order and everything it owns (its lines). All the business rules that
/// must always hold true (invariants) are enforced here -- nowhere else in the codebase
/// is allowed to mutate an Order's state directly.
///
/// Invariants enforced by this aggregate:
///   1. An order must always belong to a customer.
///   2. Lines can only be added/removed while the order is in Draft status.
///   3. An order cannot be placed with zero lines.
///   4. An order cannot be shipped unless it has been placed first.
///   5. A shipped order can never be cancelled.
///   6. All lines and the order total share a single currency.
/// </summary>
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = new();

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string Currency { get; private set; } = "USD";
    public Address? ShippingAddress { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? PlacedOnUtc { get; private set; }
    public DateTime? ShippedOnUtc { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Money Total => _lines
        .Select(l => l.LineTotal)
        .Aggregate(Money.Zero(Currency), (acc, lineTotal) => acc.Add(lineTotal));

    // EF Core needs a parameterless constructor; private so nobody outside can misuse it.
    private Order() { }

    private Order(Guid id, Guid customerId, string currency) : base(id)
    {
        CustomerId = customerId;
        Currency = currency;
        Status = OrderStatus.Draft;
        CreatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method -- the only way to create a new Order. Using a static factory
    /// instead of a public constructor lets us validate inputs and keeps construction
    /// expressive ("Order.Create(...)" reads like the Ubiquitous Language).
    /// </summary>
    public static Order Create(Guid customerId, string currency = "USD")
    {
        if (customerId == Guid.Empty)
            throw new DomainException("An order must belong to a valid customer.");

        return new Order(Guid.NewGuid(), customerId, currency);
    }

    public void AddLine(Guid productId, string productName, int quantity, Money unitPrice)
    {
        EnsureDraft("add a line to");

        if (unitPrice.Currency != Currency)
            throw new DomainException(
                $"Cannot add a line priced in {unitPrice.Currency} to an order in {Currency}.");

        var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine is not null)
        {
            // Business rule: adding the same product again increases the quantity
            // rather than creating a duplicate line.
            _lines.Remove(existingLine);
            _lines.Add(existingLine.WithQuantity(existingLine.Quantity + quantity));
        }
        else
        {
            _lines.Add(OrderLine.Create(productId, productName, quantity, unitPrice));
        }
    }

    public void RemoveLine(Guid productId)
    {
        EnsureDraft("remove a line from");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId)
            ?? throw new DomainException($"No line for product {productId} exists on this order.");

        _lines.Remove(line);
    }

    public void SetShippingAddress(Address address)
    {
        EnsureDraft("set the shipping address on");
        ShippingAddress = address;
    }

    /// <summary>
    /// Transitions the order from Draft to Placed. This is the moment the order becomes
    /// "real" from the business's point of view -- inventory can be reserved, payment
    /// can be captured, confirmation emails can go out. All of that happens elsewhere,
    /// triggered by the OrderPlacedEvent raised here; the Order itself doesn't know or
    /// care who's listening.
    /// </summary>
    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException($"Cannot place an order that is already {Status}.");

        if (_lines.Count == 0)
            throw new DomainException("Cannot place an order with no lines.");

        if (ShippingAddress is null)
            throw new DomainException("Cannot place an order without a shipping address.");

        Status = OrderStatus.Placed;
        PlacedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderPlacedEvent(Id, CustomerId, Total));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Placed)
            throw new DomainException($"Cannot ship an order that is {Status}; it must be Placed first.");

        Status = OrderStatus.Shipped;
        ShippedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderShippedEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped)
            throw new DomainException("A shipped order cannot be cancelled.");

        if (Status == OrderStatus.Cancelled)
            throw new DomainException("This order is already cancelled.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A cancellation reason is required.");

        Status = OrderStatus.Cancelled;

        RaiseDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    private void EnsureDraft(string action)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException($"Cannot {action} an order that is {Status}; only Draft orders can be modified.");
    }
}
