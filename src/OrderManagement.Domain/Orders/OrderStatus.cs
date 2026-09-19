namespace OrderManagement.Domain.Orders;

/// <summary>
/// The lifecycle states of an Order. Kept in the domain because the *rules* about
/// which transitions are allowed (e.g. you cannot ship a Draft order) are business
/// rules, not UI or database concerns.
/// </summary>
public enum OrderStatus
{
    Draft = 0,
    Placed = 1,
    Shipped = 2,
    Cancelled = 3
}
