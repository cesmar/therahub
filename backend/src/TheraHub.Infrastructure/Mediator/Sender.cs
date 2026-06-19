using TheraHub.Application.Abstractions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace TheraHub.Infrastructure.Mediator;

public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task<Result<TResult>> SendCommandAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        var behaviors = GetBehaviors<TResult>(command);

        return await ExecutePipelineAsync<TResult>(command, handler, behaviors, "HandleAsync", cancellationToken);
    }

    public async Task<Result<TResult>> SendQueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        var behaviors = GetBehaviors<TResult>(query);

        return await ExecutePipelineAsync<TResult>(query, handler, behaviors, "HandleAsync", cancellationToken);
    }

    private IEnumerable<object> GetBehaviors<TResult>(object request)
    {
        var behaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(request.GetType(), typeof(TResult));

        return _serviceProvider.GetServices(behaviorType).OfType<object>();
    }

    private static async Task<Result<TResult>> ExecutePipelineAsync<TResult>(
        object request,
        object handler,
        IEnumerable<object> behaviors,
        string methodName,
        CancellationToken cancellationToken)
    {
        Func<Task<Result<TResult>>> handlerCall = () =>
        {
            var method = handler.GetType().GetMethod(methodName)!;
            return (Task<Result<TResult>>)method.Invoke(handler, [request, cancellationToken])!;
        };

        var pipeline = behaviors
            .Reverse()
            .Aggregate(handlerCall, (next, behavior) =>
            {
                var capturedNext = next;
                return () =>
                {
                    var method = behavior.GetType().GetMethod(methodName)!;
                    return (Task<Result<TResult>>)method.Invoke(
                        behavior, [request, capturedNext, cancellationToken])!;
                };
            });

        return await pipeline();
    }
}
