namespace TheraHub.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    string? Id { get; }
    bool IsAuthenticated { get; }
}
