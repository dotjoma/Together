using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;

namespace Together.Infrastructure.Data;

/// <summary>
/// SQLite database context for offline caching
/// </summary>
public class OfflineCacheDbContext : DbContext
{
    public DbSet<OfflineOperation> OfflineOperations { get; set; } = null!;
    public DbSet<CachedPost> CachedPosts { get; set; } = null!;
    public DbSet<CachedJournalEntry> CachedJournalEntries { get; set; } = null!;
    public DbSet<CachedMoodEntry> CachedMoodEntries { get; set; } = null!;

    public OfflineCacheDbContext(DbContextOptions<OfflineCacheDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure OfflineOperation
        modelBuilder.Entity<OfflineOperation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.OperationType).IsRequired();
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.RetryCount).HasDefaultValue(0);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure CachedPost
        modelBuilder.Entity<CachedPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorUsername).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.CachedAt);
        });

        // Configure CachedJournalEntry
        modelBuilder.Entity<CachedJournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorUsername).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.ConnectionId);
            entity.HasIndex(e => e.CachedAt);
        });

        // Configure CachedMoodEntry
        modelBuilder.Entity<CachedMoodEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mood).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
