using Microsoft.EntityFrameworkCore;
using Together.Domain.Entities;

namespace Together.Infrastructure.Data;

public class TogetherDbContext : DbContext
{
    public TogetherDbContext(DbContextOptions<TogetherDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<CoupleConnection> CoupleConnections { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<MoodEntry> MoodEntries { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<SharedEvent> SharedEvents { get; set; }
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<VirtualPet> VirtualPets { get; set; }
    public DbSet<FollowRelationship> FollowRelationships { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PostImage> PostImages { get; set; }
    public DbSet<ConnectionRequest> ConnectionRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TogetherDbContext).Assembly);
    }
}
