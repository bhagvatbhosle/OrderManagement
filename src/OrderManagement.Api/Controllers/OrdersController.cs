using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Orders.Commands;
using OrderManagement.Application.Orders.DTOs;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// Deliberately thin: this controller's only job is to translate HTTP requests into
/// Application commands/queries and translate the results (or exceptions) back into
/// HTTP responses. No business logic lives here -- that's the whole point of layering.
/// </summary>
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ICommandHandler<CreateOrderCommand, OrderDto> _createOrder;
    private readonly ICommandHandler<AddOrderLineCommand, OrderDto> _addOrderLine;
    private readonly ICommandHandler<SetShippingAddressCommand, OrderDto> _setShippingAddress;
    private readonly ICommandHandler<PlaceOrderCommand, OrderDto> _placeOrder;
    private readonly ICommandHandler<ShipOrderCommand, OrderDto> _shipOrder;
    private readonly ICommandHandler<CancelOrderCommand, OrderDto> _cancelOrder;
    private readonly ICommandHandler<GetOrderByIdQuery, OrderDto> _getOrderById;

    public OrdersController(
        ICommandHandler<CreateOrderCommand, OrderDto> createOrder,
        ICommandHandler<AddOrderLineCommand, OrderDto> addOrderLine,
        ICommandHandler<SetShippingAddressCommand, OrderDto> setShippingAddress,
        ICommandHandler<PlaceOrderCommand, OrderDto> placeOrder,
        ICommandHandler<ShipOrderCommand, OrderDto> shipOrder,
        ICommandHandler<CancelOrderCommand, OrderDto> cancelOrder,
        ICommandHandler<GetOrderByIdQuery, OrderDto> getOrderById)
    {
        _createOrder = createOrder;
        _addOrderLine = addOrderLine;
        _setShippingAddress = setShippingAddress;
        _placeOrder = placeOrder;
        _shipOrder = shipOrder;
        _cancelOrder = cancelOrder;
        _getOrderById = getOrderById;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderCommand command, CancellationToken ct)
    {
        var result = await Try(() => _createOrder.Handle(command, ct));
        return result is null ? BadRequestResult() : CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _getOrderById.Handle(new GetOrderByIdQuery(id), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/lines")]
    public async Task<ActionResult<OrderDto>> AddLine(Guid id, [FromBody] AddLineRequest request, CancellationToken ct)
    {
        var command = new AddOrderLineCommand(id, request.ProductId, request.ProductName, request.Quantity, request.UnitPrice);
        return await HandleOrderCommand(() => _addOrderLine.Handle(command, ct));
    }

    [HttpPut("{id:guid}/shipping-address")]
    public async Task<ActionResult<OrderDto>> SetShippingAddress(Guid id, [FromBody] SetShippingAddressRequest request, CancellationToken ct)
    {
        var command = new SetShippingAddressCommand(
            id, request.Line1, request.Line2, request.City, request.State, request.PostalCode, request.Country);
        return await HandleOrderCommand(() => _setShippingAddress.Handle(command, ct));
    }

    [HttpPost("{id:guid}/place")]
    public async Task<ActionResult<OrderDto>> Place(Guid id, CancellationToken ct) =>
        await HandleOrderCommand(() => _placeOrder.Handle(new PlaceOrderCommand(id), ct));

    [HttpPost("{id:guid}/ship")]
    public async Task<ActionResult<OrderDto>> Ship(Guid id, CancellationToken ct) =>
        await HandleOrderCommand(() => _shipOrder.Handle(new ShipOrderCommand(id), ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, [FromBody] CancelRequest request, CancellationToken ct) =>
        await HandleOrderCommand(() => _cancelOrder.Handle(new CancelOrderCommand(id, request.Reason), ct));

    /// <summary>
    /// Central place to translate Domain/Application exceptions into HTTP status codes,
    /// so each action method doesn't repeat the same try/catch.
    /// </summary>
    private async Task<ActionResult<OrderDto>> HandleOrderCommand(Func<Task<OrderDto>> action)
    {
        try
        {
            return Ok(await action());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            // A business rule was violated (e.g. "cannot ship a draft order") --
            // that's a client error, not a server error.
            return BadRequest(new { error = ex.Message });
        }
    }

    private async Task<OrderDto?> Try(Func<Task<OrderDto>> action)
    {
        try
        {
            return await action();
        }
        catch (DomainException)
        {
            return null;
        }
    }

    private ActionResult<OrderDto> BadRequestResult() => BadRequest(new { error = "The request violated a business rule." });

    public sealed record AddLineRequest(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

    public sealed record SetShippingAddressRequest(string Line1, string? Line2, string City, string State, string PostalCode, string Country);

    public sealed record CancelRequest(string Reason);
}
