using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TheraHub.Application.Abstractions.Authentication;
using TheraHub.Domain.Common;

namespace TheraHub.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public AuditableEntityInterceptor(ICurrentUser currentUser, TimeProvider timeProvider)
    {
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        var now = _timeProvider.GetUtcNow();
        var userId = _currentUser.Id ?? "system";

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.CreatedAt).CurrentValue = now;
                    entry.Property(e => e.CreatedBy).CurrentValue = userId;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
                    entry.Property(e => e.UpdatedBy).CurrentValue = userId;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    entry.Property(e => e.IsDeleted).CurrentValue = true;
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
                    entry.Property(e => e.UpdatedBy).CurrentValue = userId;
                    break;
            }
        }
    }
}
