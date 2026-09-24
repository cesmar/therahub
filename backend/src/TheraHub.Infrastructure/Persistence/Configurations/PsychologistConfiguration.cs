using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheraHub.Domain.Entities;

namespace TheraHub.Infrastructure.Persistence.Configurations;

public class PsychologistConfiguration : IEntityTypeConfiguration<Psychologist>
{
    private readonly TheraHubDbContext _dbContext;

    public PsychologistConfiguration(TheraHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Configure(EntityTypeBuilder<Psychologist> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PublicId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(p => p.Bio)
            .HasMaxLength(1000);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.PracticeId)
            .IsRequired();

        builder.HasQueryFilter(p => p.PracticeId == _dbContext.CurrentPracticeId && !p.IsDeleted);
    }
}
