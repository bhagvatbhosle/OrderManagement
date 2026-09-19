using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Orders;
using OrderManagement.Domain.Orders.Events;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.Domain.Tests;

public class OrderTests
{
    private static Address SampleAddress() =>
        Address.Create("221B Baker Street", null, "London", "London", "NW1 6XE", "UK");

    private static Order DraftOrderWithOneLine()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(Guid.NewGuid(), "Wireless Mouse", 2, Money.Of(19.99m, "USD"));
        return order;
    }

    [Fact]
    public void New_order_starts_in_draft_status()
    {
        var order = Order.Create(Guid.NewGuid());

        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Empty(order.Lines);
    }

    [Fact]
    public void Creating_an_order_without_a_customer_throws()
    {
        Assert.Throws<DomainException>(() => Order.Create(Guid.Empty));
    }

    [Fact]
    public void Adding_the_same_product_twice_increases_quantity_instead_of_duplicating_the_line()
    {
        var order = Order.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();

        order.AddLine(productId, "USB-C Cable", 1, Money.Of(9.99m, "USD"));
        order.AddLine(productId, "USB-C Cable", 2, Money.Of(9.99m, "USD"));

        var line = Assert.Single(order.Lines);
        Assert.Equal(3, line.Quantity);
    }

    [Fact]
    public void Total_is_the_sum_of_all_line_totals()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(Guid.NewGuid(), "Keyboard", 1, Money.Of(49.99m, "USD"));
        order.AddLine(Guid.NewGuid(), "Mouse", 2, Money.Of(19.99m, "USD"));

        Assert.Equal(Money.Of(89.97m, "USD"), order.Total);
    }

    [Fact]
    public void Cannot_place_an_order_with_no_lines()
    {
        var order = Order.Create(Guid.NewGuid());
        order.SetShippingAddress(SampleAddress());

        Assert.Throws<DomainException>(() => order.Place());
    }

    [Fact]
    public void Cannot_place_an_order_without_a_shipping_address()
    {
        var order = DraftOrderWithOneLine();

        Assert.Throws<DomainException>(() => order.Place());
    }

    [Fact]
    public void Placing_a_valid_order_raises_OrderPlacedEvent_and_changes_status()
    {
        var order = DraftOrderWithOneLine();
        order.SetShippingAddress(SampleAddress());

        order.Place();

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.NotNull(order.PlacedOnUtc);
        var domainEvent = Assert.Single(order.DomainEvents);
        Assert.IsType<OrderPlacedEvent>(domainEvent);
    }

    [Fact]
    public void Cannot_add_a_line_to_an_order_that_is_no_longer_draft()
    {
        var order = DraftOrderWithOneLine();
        order.SetShippingAddress(SampleAddress());
        order.Place();

        Assert.Throws<DomainException>(() =>
            order.AddLine(Guid.NewGuid(), "Extra Item", 1, Money.Of(5m, "USD")));
    }

    [Fact]
    public void Cannot_ship_an_order_that_has_not_been_placed()
    {
        var order = DraftOrderWithOneLine();

        Assert.Throws<DomainException>(() => order.Ship());
    }

    [Fact]
    public void Shipping_a_placed_order_succeeds_and_raises_event()
    {
        var order = DraftOrderWithOneLine();
        order.SetShippingAddress(SampleAddress());
        order.Place();
        order.ClearDomainEvents();

        order.Ship();

        Assert.Equal(OrderStatus.Shipped, order.Status);
        var domainEvent = Assert.Single(order.DomainEvents);
        Assert.IsType<OrderShippedEvent>(domainEvent);
    }

    [Fact]
    public void A_shipped_order_cannot_be_cancelled()
    {
        var order = DraftOrderWithOneLine();
        order.SetShippingAddress(SampleAddress());
        order.Place();
        order.Ship();

        Assert.Throws<DomainException>(() => order.Cancel("Changed my mind"));
    }

    [Fact]
    public void Cancelling_a_placed_order_raises_OrderCancelledEvent()
    {
        var order = DraftOrderWithOneLine();
        order.SetShippingAddress(SampleAddress());
        order.Place();
        order.ClearDomainEvents();

        order.Cancel("Customer changed their mind");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        var domainEvent = Assert.Single(order.DomainEvents);
        var cancelledEvent = Assert.IsType<OrderCancelledEvent>(domainEvent);
        Assert.Equal("Customer changed their mind", cancelledEvent.Reason);
    }

    [Fact]
    public void Adding_a_line_priced_in_a_different_currency_than_the_order_throws()
    {
        var order = Order.Create(Guid.NewGuid(), currency: "USD");

        Assert.Throws<DomainException>(() =>
            order.AddLine(Guid.NewGuid(), "Imported Gadget", 1, Money.Of(15m, "EUR")));
    }
}
