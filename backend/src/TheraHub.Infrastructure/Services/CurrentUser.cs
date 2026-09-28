using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TheraHub.Application.Abstractions.Authentication;

namespace TheraHub.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ASP.NET Core's JwtBearerHandler remaps the "sub" claim to ClaimTypes.NameIdentifier by
    // default, so we check both to work regardless of how #21 configures MapInboundClaims.
    public string? Id =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
