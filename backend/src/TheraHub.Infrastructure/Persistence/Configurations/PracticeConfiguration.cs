using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheraHub.Domain.Entities;

namespace TheraHub.Infrastructure.Persistence.Configurations;

public class PracticeConfiguration : IEntityTypeConfiguration<Practice>
{
    public void Configure(EntityTypeBuilder<Practice> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PublicId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.ActivatedAt);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
