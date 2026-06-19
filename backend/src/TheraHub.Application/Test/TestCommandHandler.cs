using TheraHub.Application.Abstractions.Mediator;

namespace TheraHub.Application.Test;

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Success($"Hello, {command.Name}"));
    }
}
