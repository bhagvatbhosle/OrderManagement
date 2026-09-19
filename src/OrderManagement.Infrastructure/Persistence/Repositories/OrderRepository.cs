using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Orders;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// The real, EF Core-backed implementation of IOrderRepository. This is the only class
/// in the whole solution that knows Orders live in a SQL table -- everything upstream
/// (Application, Api) only ever talks to the IOrderRepository abstraction.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderManagementDbContext _dbContext;

    public OrderRepository(OrderManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include the owned OrderLines collection explicitly so it's loaded in one query.
        return await _dbContext.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Lines)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        // EF Core is already tracking 'order' (it was loaded via GetByIdAsync in the
        // same DbContext scope), so there's nothing extra to do here. This method exists
        // to keep the repository's contract explicit and to support implementations
        // (e.g. a different ORM, or a detached-entity scenario) where it would matter.
        _dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
