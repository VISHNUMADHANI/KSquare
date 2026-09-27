using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KsSquare.Persistence.Configurations;
public sealed class PayPalAttemptConfiguration:IEntityTypeConfiguration<PayPalAttempt>
{
 public void Configure(EntityTypeBuilder<PayPalAttempt> b){b.ToTable("paypal_attempts");b.HasKey(x=>x.Id);b.HasIndex(x=>new{x.CustomerId,x.CheckoutKey}).IsUnique();b.HasIndex(x=>x.CustomerId).IsUnique().HasFilter("state NOT IN ('Completed', 'Abandoned')");b.HasIndex(x=>x.PayPalOrderId).IsUnique();b.HasIndex(x=>x.CaptureId).IsUnique();b.HasIndex(x=>x.CustomRequestId).IsUnique().HasFilter("state <> 'Abandoned'");b.Property(x=>x.Total).HasPrecision(18,2);b.Property(x=>x.Currency).HasMaxLength(3);b.Property(x=>x.Environment).HasMaxLength(10);b.Property(x=>x.State).HasMaxLength(20);b.Property(x=>x.RequestHash).HasMaxLength(64);b.Property(x=>x.PayPalOrderId).HasMaxLength(64);b.Property(x=>x.CaptureId).HasMaxLength(64);b.Property(x=>x.ApprovalUrl).HasMaxLength(2048);b.Property(x=>x.CheckoutJson).HasColumnType("jsonb");b.Property(x=>x.ItemsJson).HasColumnType("jsonb");b.Property(x=>x.ShippingJson).HasColumnType("jsonb");b.HasOne<AppUser>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);b.HasOne<CustomerOrder>().WithMany().HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);}
}
