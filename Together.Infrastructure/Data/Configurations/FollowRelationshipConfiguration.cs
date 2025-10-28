using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Together.Domain.Entities;

namespace Together.Infrastructure.Data.Configurations;

public class FollowRelationshipConfiguration : IEntityTypeConfiguration<FollowRelationship>
{
    public void Configure(EntityTypeBuilder<FollowRelationship> builder)
    {
        builder.ToTable("follow_relationships");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(f => f.FollowerId)
            .HasColumnName("follower_id")
            .IsRequired();

        builder.Property(f => f.FollowingId)
            .HasColumnName("following_id")
            .IsRequired();

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.AcceptedAt)
            .HasColumnName("accepted_at");

        // Indexes
        builder.HasIndex(f => f.FollowerId);
        builder.HasIndex(f => f.FollowingId);
        builder.HasIndex(f => new { f.FollowerId, f.FollowingId })
            .IsUnique();
    }
}
