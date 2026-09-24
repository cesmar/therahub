using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheraHub.Domain.Entities;

namespace TheraHub.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private readonly TheraHubDbContext _dbContext;
    //private readonly ICurrentTenant _currentTenant;

    public UserConfiguration(TheraHubDbContext dbContext
        //, ICurrentTenant currentTenant
        )
    {
        _dbContext = dbContext;
        //_currentTenant = currentTenant;
    }

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.PublicId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.IsEmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.PracticeId)
            .IsRequired();

        builder.HasQueryFilter(u => u.PracticeId == _dbContext.CurrentPracticeId && !u.IsDeleted);

        //builder.HasQueryFilter(u => u.PracticeId == _currentTenant.PracticeId && !u.IsDeleted);
        // Previous approach (do NOT use):
        // builder.HasQueryFilter(u => u.PracticeId == _currentTenant.PracticeId && !u.IsDeleted);
        // EF Core builds and caches the model once, so capturing ICurrentTenant here binds the filter
        // to the instance that built the model. Referencing a DbContext member makes EF evaluate it
        // against the current context on every query.
    }
}
