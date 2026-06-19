namespace TheraHub.Application.Abstractions.Mediator;

public interface IPipelineBehavior<TRequest, TResult>
{
    Task<Result<TResult>> HandleAsync(
        TRequest request,
        Func<Task<Result<TResult>>> next,
        CancellationToken cancellationToken = default);
}
