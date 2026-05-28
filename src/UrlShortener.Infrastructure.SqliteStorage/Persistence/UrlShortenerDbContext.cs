using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Infrastructure.SqliteStorage.Persistence;

internal sealed class UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : DbContext(options)
{
    public DbSet<UrlMappingRecord> UrlMappings => Set<UrlMappingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UrlMappingRecord>();
        entity.ToTable("UrlMappings");
        entity.HasKey(x => x.ShortUrl);
        entity.Property(x => x.ShortUrl).HasMaxLength(256);
        entity.Property(x => x.OriginalUrl).IsRequired();
        entity.Property(x => x.CreatedAt).IsRequired();
    }
}
