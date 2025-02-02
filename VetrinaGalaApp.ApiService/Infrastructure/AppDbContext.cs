using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetrinaGalaApp.ApiService.Domain;

namespace VetrinaGalaApp.ApiService.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Discount> Discounts => Set<Discount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User Configuration
        builder.Entity<User>()
            .Property(x => x.UserType)
            .HasConversion<string>();

        // Store Configuration
        builder.Entity<Store>()
            .HasOne(s => s.User)
            .WithOne(u => u.Store)
            .HasForeignKey<Store>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item Configuration
        builder.Entity<Item>()
            .HasOne(i => i.Store)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Discount Configuration
        builder.Entity<Discount>()
            .HasOne(d => d.Store)
            .WithMany(s => s.Discounts)
            .HasForeignKey(d => d.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Discount>()
            .HasOne(d => d.Item)
            .WithMany(i => i.Discounts)
            .HasForeignKey(d => d.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Catalog Configuration

        builder.Entity<Catalog>()
            .Property(x => x.Type)
            .HasConversion<string>();

        builder.Entity<Catalog>()
            .HasOne<Store>()
            .WithMany(s => s.Catalogs)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Catalog>()
            .HasMany(i => i.Items)
            .WithOne(c => c.Catalog)
            .HasForeignKey(i => i.CatalogId);
        
    }
}

