namespace OrderManagement.Application.Abstractions;

/// <summary>
/// A tiny "use case" abstraction. Each application use case (Place an order, Ship an
/// order, ...) gets its own command + handler. This keeps orchestration logic out of
/// the Domain (which should stay focused purely on business rules) and out of the Api
/// layer (which should stay focused on HTTP concerns). If the project grows, swapping
/// this for MediatR's IRequestHandler is a drop-in replacement.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
