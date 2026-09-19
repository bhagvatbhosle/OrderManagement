using OrderManagement.Domain.Common;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Domain.ValueObjects;

/// <summary>
/// A shipping/billing address. Like all Value Objects, it has no identity of its own --
/// it's just a meaningful grouping of attributes, validated as a whole at construction time.
/// </summary>
public sealed class Address : ValueObject
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string line1, string? line2, string city, string state, string postalCode, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string line1, string? line2, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(line1)) throw new DomainException("Address line 1 is required.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(state)) throw new DomainException("State is required.");
        if (string.IsNullOrWhiteSpace(postalCode)) throw new DomainException("Postal code is required.");
        if (string.IsNullOrWhiteSpace(country)) throw new DomainException("Country is required.");

        return new Address(line1.Trim(), line2?.Trim(), city.Trim(), state.Trim(), postalCode.Trim(), country.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Line1}, {(Line2 is null ? "" : Line2 + ", ")}{City}, {State} {PostalCode}, {Country}";
}
