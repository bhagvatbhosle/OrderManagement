namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would violate one of the domain's business rules (invariants).
/// Using a dedicated exception type (rather than a generic InvalidOperationException) makes
/// it easy for the Application/Api layers to catch domain-rule violations specifically and
/// translate them into, e.g., a 400 Bad Request instead of a 500.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
