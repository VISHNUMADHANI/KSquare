using KsSquare.Domain.Common;
using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KsSquare.Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductChainVariant> ProductChainVariants => Set<ProductChainVariant>();
    public DbSet<ProductPendantVariant> ProductPendantVariants => Set<ProductPendantVariant>();
    public DbSet<ProductBraceletVariant> ProductBraceletVariants => Set<ProductBraceletVariant>();
    public DbSet<CustomJewelryRequest> CustomJewelryRequests => Set<CustomJewelryRequest>();
    public DbSet<CustomJewelryRequestMedia> CustomJewelryRequestMedia => Set<CustomJewelryRequestMedia>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges() { ApplyAuditTimestamps(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { ApplyAuditTimestamps(); return base.SaveChangesAsync(cancellationToken); }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.SetCreatedAt(utcNow);
            else if (entry.State == EntityState.Modified) entry.Entity.SetUpdatedAt(utcNow);
        }
    }
}
