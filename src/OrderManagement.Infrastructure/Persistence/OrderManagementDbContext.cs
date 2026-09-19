using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Customers;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// The EF Core DbContext. Notice that the Domain classes themselves have NO EF Core
/// attributes ([Key], [Required], etc.) on them -- all mapping is done here, via the
/// Fluent API, in the Configurations/ folder. This is what "persistence ignorance"
/// means in practice: you could throw this whole class away and use Dapper, MongoDB,
/// or an in-memory store instead, and the Domain project wouldn't need a single change.
/// </summary>
public sealed class OrderManagementDbContext : DbContext
{
    public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderManagementDbContext).Assembly);
    }
}
