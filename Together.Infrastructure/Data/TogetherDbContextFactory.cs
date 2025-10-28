using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Together.Infrastructure.Data;

public class TogetherDbContextFactory : IDesignTimeDbContextFactory<TogetherDbContext>
{
    public TogetherDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TogetherDbContext>();
        
        // Use a placeholder connection string for migrations
        // This will be replaced with the actual connection string from appsettings.json at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=together;Username=postgres;Password=postgres");

        return new TogetherDbContext(optionsBuilder.Options);
    }
}
