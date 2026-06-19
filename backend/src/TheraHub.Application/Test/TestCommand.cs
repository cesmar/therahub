using TheraHub.Application.Abstractions.Mediator;

namespace TheraHub.Application.Test;

public record TestCommand(string Name) : ICommand<string>;
