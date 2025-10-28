using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Together.Domain.Entities;
using Together.Domain.Enums;

namespace Together.Infrastructure.Data.Configurations;

public class MoodEntryConfiguration : IEntityTypeConfiguration<MoodEntry>
{
    public void Configure(EntityTypeBuilder<MoodEntry> builder)
    {
        builder.ToTable("mood_entries");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(m => m.Mood)
            .HasColumnName("mood")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToLower(),
                v => Enum.Parse<MoodType>(v, true))
            .IsRequired();

        builder.Property(m => m.Notes)
            .HasColumnName("notes");

        builder.Property(m => m.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        // Indexes
        builder.HasIndex(m => m.UserId);
        builder.HasIndex(m => m.Timestamp);
    }
}
