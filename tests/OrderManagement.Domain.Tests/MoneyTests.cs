using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Two_money_objects_with_same_amount_and_currency_are_equal()
    {
        var a = Money.Of(10.00m, "USD");
        var b = Money.Of(10.00m, "usd"); // currency comparison should be case-insensitive

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Adding_money_in_different_currencies_throws()
    {
        var usd = Money.Of(10m, "USD");
        var eur = Money.Of(10m, "EUR");

        Assert.Throws<DomainException>(() => usd.Add(eur));
    }

    [Fact]
    public void Negative_amount_is_rejected()
    {
        Assert.Throws<DomainException>(() => Money.Of(-1m, "USD"));
    }

    [Fact]
    public void Multiply_scales_the_amount_correctly()
    {
        var price = Money.Of(9.99m, "USD");
        var lineTotal = price.Multiply(3);

        Assert.Equal(Money.Of(29.97m, "USD"), lineTotal);
    }

    [Fact]
    public void Subtract_below_zero_throws()
    {
        var a = Money.Of(5m, "USD");
        var b = Money.Of(10m, "USD");

        Assert.Throws<DomainException>(() => a.Subtract(b));
    }
}
