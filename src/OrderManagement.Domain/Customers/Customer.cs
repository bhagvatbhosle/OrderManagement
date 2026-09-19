using OrderManagement.Domain.Common;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Customers;

/// <summary>
/// Customer is modeled as a simple Entity (not an aggregate root with complex behavior
/// here) because, for this bounded context, all we care about is identity + a few
/// attributes. In a larger system, Customer might be its own aggregate root with its
/// own invariants (e.g. loyalty tier rules) -- keep aggregates as small as the business
/// rules require, no smaller and no bigger.
/// </summary>
public sealed class Customer : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Address? DefaultShippingAddress { get; private set; }

    private Customer() { }

    private Customer(Guid id, string name, string email) : base(id)
    {
        Name = name;
        Email = email;
    }

    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid customer email is required.");

        return new Customer(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant());
    }

    public void SetDefaultShippingAddress(Address address) => DefaultShippingAddress = address;
}
