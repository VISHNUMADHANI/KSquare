using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KsSquare.Persistence.Configurations;

public sealed class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
{
    public void Configure(EntityTypeBuilder<CustomerOrder> b)
    {
        b.Property(x=>x.Courier).HasMaxLength(20); b.Property(x=>x.TrackingNumber).HasMaxLength(80);
        b.ToTable("customer_orders"); b.HasKey(x => x.Id);
        b.Property(x => x.OrderNumber).HasMaxLength(32); b.HasIndex(x => x.OrderNumber).IsUnique();
        b.HasIndex(x => new { x.CustomerId, x.CheckoutKey }).IsUnique();
        b.HasIndex(x => x.CustomRequestId).IsUnique();
        b.HasIndex(x => new { x.CustomerId, x.CreatedAt });
        b.HasOne<AppUser>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.RequestHash).HasMaxLength(64);
        b.Property(x => x.CustomerName).HasMaxLength(150); b.Property(x => x.CustomerEmail).HasMaxLength(254);
        b.Property(x => x.CustomerPhone).HasMaxLength(32); b.Property(x => x.DeliveryAddress).HasMaxLength(600);
        b.Property(x => x.ItemsJson).HasColumnType("jsonb"); b.Property(x => x.Total).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3); b.Property(x => x.PaymentStatus).HasMaxLength(24);
        b.Property(x => x.PaymentReference).HasMaxLength(64); b.Property(x => x.Status).HasMaxLength(24);
        b.Property(x => x.ReturnsJson).HasColumnType("jsonb");
        b.Property(x => x.Version).IsConcurrencyToken();
    }
}
