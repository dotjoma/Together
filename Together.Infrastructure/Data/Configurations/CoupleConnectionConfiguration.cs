using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Together.Domain.Entities;
using Together.Domain.Enums;

namespace Together.Infrastructure.Data.Configurations;

public class CoupleConnectionConfiguration : IEntityTypeConfiguration<CoupleConnection>
{
    public void Configure(EntityTypeBuilder<CoupleConnection> builder)
    {
        builder.ToTable("couple_connections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.User1Id)
            .HasColumnName("user1_id")
            .IsRequired();

        builder.Property(c => c.User2Id)
            .HasColumnName("user2_id")
            .IsRequired();

        builder.Property(c => c.EstablishedAt)
            .HasColumnName("established_at")
            .IsRequired();

        builder.Property(c => c.RelationshipStartDate)
            .HasColumnName("relationship_start_date")
            .IsRequired();

        builder.Property(c => c.LoveStreak)
            .HasColumnName("love_streak")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(c => c.LastInteractionDate)
            .HasColumnName("last_interaction_date");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToLower(),
                v => Enum.Parse<ConnectionStatus>(v, true))
            .IsRequired();

        builder.Property(c => c.NextMeetingDate)
            .HasColumnName("next_meeting_date");

        // Indexes
        builder.HasIndex(c => c.User1Id);
        builder.HasIndex(c => c.User2Id);

        // Relationships
        builder.HasOne(c => c.User1)
            .WithMany()
            .HasForeignKey(c => c.User1Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User2)
            .WithMany()
            .HasForeignKey(c => c.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.VirtualPet)
            .WithOne(v => v.Connection)
            .HasForeignKey<VirtualPet>(v => v.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.JournalEntries)
            .WithOne(j => j.Connection)
            .HasForeignKey(j => j.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.TodoItems)
            .WithOne(t => t.Connection)
            .HasForeignKey(t => t.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Events)
            .WithOne(e => e.Connection)
            .HasForeignKey(e => e.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Challenges)
            .WithOne(ch => ch.Connection)
            .HasForeignKey(ch => ch.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
