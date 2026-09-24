using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TheraHub.Application.Abstractions.Tenancy;
using TheraHub.Domain.Entities;
using TheraHub.Infrastructure.Persistence.Configurations;

namespace TheraHub.Infrastructure.Persistence;

public class TheraHubDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public TheraHubDbContext(DbContextOptions<TheraHubDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Practice> Practices => Set<Practice>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Psychologist> Psychologists => Set<Psychologist>();

    // Referenced (not captured) by tenant query filters — see UserConfiguration/PsychologistConfiguration.
    // EF Core rebinds expressions rooted at a DbContext-typed member to the current context instance at
    // query time, even though the model itself is built once and cached. Capturing ICurrentTenant directly
    // in the filter would bake in whichever instance happened to build the model.
    public int CurrentPracticeId => _currentTenant.PracticeId;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplyConfiguration(new UserConfiguration(this));
        builder.ApplyConfiguration(new PsychologistConfiguration(this));
    }
}
