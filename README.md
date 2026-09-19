# OrderManagement — a from-scratch DDD starter (.NET 8 / C#)

A small Order Management aggregate, built to teach Domain-Driven Design fundamentals
by example rather than just definition. Read the code top-down: `Domain` first, then
`Application`, then `Infrastructure`, then `Api`.

## Solution structure

```
OrderManagement.sln
src/
  OrderManagement.Domain/          <- Pure C#. Zero package references. This is DDD's core.
    Common/                        <- Entity, ValueObject, AggregateRoot, IDomainEvent base types
    Orders/                        <- The Order aggregate: Order, OrderLine, OrderStatus, events
    Customers/                     <- Customer entity
    ValueObjects/                  <- Money, Address
    Exceptions/                    <- DomainException

  OrderManagement.Application/     <- Use cases that orchestrate the Domain
    Orders/Commands/               <- CreateOrder, AddOrderLine, PlaceOrder, ShipOrder, CancelOrder, GetOrderById
    Orders/DTOs/                   <- OrderDto (never leak Domain objects across the boundary)
    Abstractions/                  <- ICommandHandler<>, IDomainEventDispatcher

  OrderManagement.Infrastructure/  <- EF Core + SQLite implementation of the Domain's interfaces
    Persistence/                   <- DbContext, Fluent API configurations, repositories

  OrderManagement.Api/             <- Thin ASP.NET Core Web API (controllers only translate HTTP <-> commands)

tests/
  OrderManagement.Domain.Tests/    <- xUnit tests proving the aggregate's invariants actually hold
```

## The Order aggregate's business rules (invariants)

These live entirely inside `Order.cs` — not in the controller, not in a validator, not
in the database:

1. An order must always belong to a valid customer.
2. Lines can only be added or removed while the order is `Draft`.
3. An order cannot be placed with zero lines, or without a shipping address.
4. An order cannot be shipped unless it has already been placed.
5. A shipped order can never be cancelled.
6. Every line and the order total share a single currency.

Try to break one of these from outside `Order` — you can't. That's the point of an
Aggregate Root: it's the only door in, and it guards the rules.

## Running it

You'll need the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
# From the solution root:
dotnet restore
dotnet build

# Run the domain unit tests (no database needed):
dotnet test tests/OrderManagement.Domain.Tests

# Run the API (creates a local ordermanagement.db SQLite file automatically):
dotnet run --project src/OrderManagement.Api
```

Then open the Swagger UI (URL printed in the console, typically `https://localhost:xxxx/swagger`)
and try this sequence:

1. `POST /api/orders` — `{ "customerId": "<any-guid>", "currency": "USD" }`
2. `POST /api/orders/{id}/lines` — add a product line
3. `PUT /api/orders/{id}/shipping-address` — set an address
4. `POST /api/orders/{id}/place` — place the order (fails until steps 2 & 3 are done — try skipping them!)
5. `POST /api/orders/{id}/ship` — ship it
6. `POST /api/orders/{id}/cancel` — try this *after* shipping and watch it correctly fail with 400

## Suggested learning path through this codebase

1. **Start in `Domain/Common`** — understand `Entity`, `ValueObject`, `AggregateRoot`.
2. **Read `ValueObjects/Money.cs`** — see how immutability + validation work together.
3. **Read `Orders/Order.cs`** top to bottom** — this is the whole DDD story in one file:
   factory method, invariant checks, state transitions, domain events.
4. **Read `tests/OrderManagement.Domain.Tests/OrderTests.cs`** — see the invariants proven.
5. **Read `Application/Orders/Commands/PlaceOrderCommand.cs`** — see how a use case
   orchestrates the aggregate + repository + event dispatcher without containing any
   business logic itself.
6. **Read `Infrastructure/Persistence/Configurations/OrderConfiguration.cs`** — see how
   EF Core maps a rich domain model without a single attribute polluting the Domain project.

## Natural next steps once you're comfortable

- Add an `Inventory` bounded context (own `Product`/`Stock` model) that reacts to
  `OrderPlacedEvent` — this is where you'll feel *why* Bounded Contexts matter.
- Swap `InProcessDomainEventDispatcher` for MediatR notifications.
- Add optimistic concurrency (a `RowVersion` column) to `Order` to handle concurrent updates.
- Add integration tests using `WebApplicationFactory` against an in-memory SQLite database.
- Introduce a `Specification` pattern for more complex queries (e.g. "orders placed in the last 30 days").
