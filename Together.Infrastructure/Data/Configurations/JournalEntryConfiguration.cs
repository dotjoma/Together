using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Together.Domain.Entities;

namespace Together.Infrastructure.Data.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(j => j.ConnectionId)
            .HasColumnName("connection_id")
            .IsRequired();

        builder.Property(j => j.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(j => j.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.IsReadByPartner)
            .HasColumnName("is_read_by_partner")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(j => j.ImageUrl)
            .HasColumnName("image_url");

        // Indexes
        builder.HasIndex(j => j.ConnectionId);
        builder.HasIndex(j => j.AuthorId);
        builder.HasIndex(j => j.CreatedAt);

        // Relationships
        builder.HasOne(j => j.Author)
            .WithMany()
            .HasForeignKey(j => j.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
