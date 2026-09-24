using Microsoft.AspNetCore.Http;
using TheraHub.Application.Abstractions.Tenancy;

namespace TheraHub.Infrastructure.Services;

public class CurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int PracticeId =>
        int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirst("practiceId")!.Value);
}
