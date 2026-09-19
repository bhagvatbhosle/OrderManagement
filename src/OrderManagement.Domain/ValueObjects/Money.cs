using OrderManagement.Domain.Common;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Domain.ValueObjects;

/// <summary>
/// Represents an amount of money in a specific currency. Immutable: every operation
/// returns a new Money instance rather than mutating the current one. Deliberately
/// refuses to mix currencies -- that's a business rule, not a technical detail.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency must be provided.");

        if (currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code (e.g. USD, EUR, INR).");

        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");

        // Round to avoid floating-precision surprises when persisting/serializing.
        return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => Of(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainException("Resulting money amount cannot be negative.");

        return Of(result, Currency);
    }

    public Money Multiply(int factor)
    {
        if (factor < 0)
            throw new DomainException("Cannot multiply money by a negative factor.");

        return Of(Amount * factor, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException(
                $"Cannot operate on Money in different currencies ({Currency} vs {other.Currency}).");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
