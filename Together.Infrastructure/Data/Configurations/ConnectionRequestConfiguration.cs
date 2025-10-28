using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Together.Domain.Entities;

namespace Together.Infrastructure.Data.Configurations;

public class ConnectionRequestConfiguration : IEntityTypeConfiguration<ConnectionRequest>
{
    public void Configure(EntityTypeBuilder<ConnectionRequest> builder)
    {
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.FromUserId)
            .IsRequired();

        builder.Property(cr => cr.ToUserId)
            .IsRequired();

        builder.Property(cr => cr.CreatedAt)
            .IsRequired();

        builder.Property(cr => cr.Status)
            .IsRequired()
            .HasConversion<string>();

        // Relationships
        builder.HasOne(cr => cr.FromUser)
            .WithMany()
            .HasForeignKey(cr => cr.FromUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.ToUser)
            .WithMany()
            .HasForeignKey(cr => cr.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(cr => cr.FromUserId);
        builder.HasIndex(cr => cr.ToUserId);
        builder.HasIndex(cr => cr.Status);
        builder.HasIndex(cr => new { cr.FromUserId, cr.ToUserId });
    }
}
