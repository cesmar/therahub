namespace TheraHub.Application.Abstractions.Mediator;

public interface ISender
{
    Task<Result<TResult>> SendCommandAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default);

    Task<Result<TResult>> SendQueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default);
}
