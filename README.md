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

## Containerizing the API

Build and run the API in a container:

```bash
docker build -t ordermanagement-api .
docker run --rm -p 8080:8080 -v ordermanagement-data:/app/data ordermanagement-api
```

The container listens on port 8080 and stores the SQLite database under `/app/data` to keep data persistent when using a named volume.

## Helm chart

The repository includes a Helm chart under `helm/ordermanagement-api`.

```bash
helm install ordermanagement ./helm/ordermanagement-api --set image.repository=ordermanagement-api --set image.tag=latest
```

The chart creates a deployment, a service, and a PVC for the SQLite database. By default it mounts the database at `/data/ordermanagement.db`.

Then open the Swagger UI (URL printed in the console, typically `http://localhost:8080/swagger` when running locally or via container)
and try this sequence:

1. `POST /api/orders` — `{ "customerId": "<any-guid>", "currency": "USD" }`
2. `POST /api/orders/{id}/lines` — add a product line
3. `PUT /api/orders/{id}/shipping-address` — set an address
4. `POST /api/orders/{id}/place` — place the order (fails until steps 2 & 3 are done — try skipping them!)
5. `POST /api/orders/{id}/ship` — ship it
6. `POST /api/orders/{id}/cancel` — try this *after* shipping and watch it correctly fail with 400

## Interview Preparation

### What is DDD?

One-liner: "Domain-Driven Design is an approach where you design your code around the actual business problem, using the same language the business uses — instead of designing around the database or the framework."

If they want more: "The idea is: talk to whoever understands the business — a product owner, a domain expert — and use their exact vocabulary in your code. If they call it an 'order being placed,' your code should literally have a method called Place(), not UpdateStatus(2). That shared vocabulary is called the Ubiquitous Language, and it's supposed to make the code readable to both developers and business people."

### What is the use of it / why does it matter?

One-liner: "It keeps business rules in one place — inside the model — instead of scattered across controllers, services, and validation layers where they're easy to duplicate or forget."

If they want more: "Without DDD, I've seen the same rule — like 'an order can't be shipped before it's paid' — get checked in the API layer, then again in a service, then maybe not at all in a background job that was added later. With DDD, that rule lives on the Order object itself, so every code path that touches an order automatically respects it. It also just makes the code easier to talk about — you can point at a class and it maps to a real business concept, not an abstract data structure."

### When would you use it?

One-liner: "When the business logic is genuinely complex — lots of rules, workflows, and edge cases. I wouldn't reach for it on a simple CRUD app."

If they want more: "If an app is basically 'save this form to a table, show it back,' DDD adds structure you don't need — it's overhead for no payoff. But once you have real business rules — things like 'you can't cancel an order after it ships,' multiple states, calculations, approval workflows — that's when putting the rules inside the domain model actually pays off. A good rule of thumb I use: if I find myself writing the same validation in two different places, that's a sign the logic belongs in the domain model instead."

### Have you used it in your project?

One-liner: "Yes — I built an Order Management system in .NET where an Order enforces its own rules, like not being shippable before it's placed, or not cancellable after it's shipped."

If they want more, this is where you drop into your actual project — the layering (Domain/Application/Infrastructure/Api), the Order aggregate example, and the "I could swap the database without touching business logic" point from before. Since you've already got that answer solid, just bridge into it naturally: "Let me walk you through it..."

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

