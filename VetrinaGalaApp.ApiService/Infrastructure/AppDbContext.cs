using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Swipe> Swipes => Set<Swipe>();

    //TODO: move the configs in their own files
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
        builder.Entity<Discount>(entity =>
        {
            // Store relationship
            entity.HasOne(d => d.Store)
            .WithMany(s => s.Discounts)
            .HasForeignKey(d => d.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

            // Item relationship
            entity.HasOne(d => d.Item)
                .WithMany(i => i.Discounts)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // Catalog Configuration

        builder.Entity<Catalog>(entity =>
        {
            entity.Property(x => x.Type)
            .HasConversion<string>();

            // Item relationship
            entity.HasMany(i => i.Items)
            .WithOne(c => c.Catalog)
            .HasForeignKey(i => i.CatalogId)
            .OnDelete(DeleteBehavior.Restrict);
        });
            

        // Swipe Configuration

        builder.Entity<Swipe>(entity =>
        {
            // User relationship
            entity.HasOne(s => s.User)
                .WithMany(u => u.Swipes)
                .HasForeignKey(s => s.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Item relationship
            entity.HasOne(s => s.Item)
                .WithMany()
                .HasForeignKey(s => s.ItemId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.ItemId);

        });

    }
}

