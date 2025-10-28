using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Together.Domain.Entities;
using Together.Domain.Enums;

namespace Together.Infrastructure.Data.Configurations;

public class VirtualPetConfiguration : IEntityTypeConfiguration<VirtualPet>
{
    public void Configure(EntityTypeBuilder<VirtualPet> builder)
    {
        builder.ToTable("virtual_pets");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(v => v.ConnectionId)
            .HasColumnName("connection_id")
            .IsRequired();

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Level)
            .HasColumnName("level")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(v => v.ExperiencePoints)
            .HasColumnName("experience_points")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(v => v.AppearanceOptions)
            .HasColumnName("appearance_options")
            .HasColumnType("jsonb");

        builder.Property(v => v.State)
            .HasColumnName("state")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToLower(),
                v => Enum.Parse<PetState>(v, true))
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(v => v.ConnectionId)
            .IsUnique();
    }
}
