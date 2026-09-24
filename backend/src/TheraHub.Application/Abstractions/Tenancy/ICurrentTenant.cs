namespace TheraHub.Application.Abstractions.Tenancy;

public interface ICurrentTenant
{
    int PracticeId { get; }
}
