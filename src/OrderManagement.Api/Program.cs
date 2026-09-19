using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.Commands;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Infrastructure (DbContext, repositories, event dispatcher) ---
builder.Services.AddInfrastructure(builder.Configuration);

// --- Application use cases ---
// Each use case is registered explicitly. With more of these, consider
// Scrutor's assembly scanning, or swap ICommandHandler<,> for MediatR.
builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, OrderDto>, CreateOrderCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddOrderLineCommand, OrderDto>, AddOrderLineCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SetShippingAddressCommand, OrderDto>, SetShippingAddressCommandHandler>();
builder.Services.AddScoped<ICommandHandler<PlaceOrderCommand, OrderDto>, PlaceOrderCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ShipOrderCommand, OrderDto>, ShipOrderCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CancelOrderCommand, OrderDto>, CancelOrderCommandHandler>();
builder.Services.AddScoped<ICommandHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdQueryHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-create the SQLite database on startup for this starter project.
// In a real project, use `dotnet ef migrations` + `dbContext.Database.Migrate()` instead.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests, if you add them later.
public partial class Program { }
