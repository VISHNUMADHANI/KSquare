using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KsSquare.Persistence.Configurations;
public sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
 public void Configure(EntityTypeBuilder<ProductReview> b)
 {
  b.HasOne<Category>().WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
  b.HasIndex(x=>new{x.CategoryId,x.Status});
  b.ToTable("product_reviews"); b.HasKey(x=>x.Id);
  b.HasIndex(x=>new{x.ProductId,x.CustomerId}).IsUnique();
  b.HasIndex(x=>new{x.ProductId,x.Status,x.CreatedAt});
  b.HasIndex(x=>new{x.Status,x.CreatedAt});
  b.Property(x=>x.DisplayName).HasMaxLength(80); b.Property(x=>x.Title).HasMaxLength(120);
  b.Property(x=>x.Body).HasMaxLength(2000); b.Property(x=>x.Status).HasMaxLength(20);
  b.Property(x=>x.ModerationNote).HasMaxLength(500); b.Property(x=>x.MediaJson).HasColumnType("jsonb");
  b.Property(x=>x.Version).IsConcurrencyToken();
  b.HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
  b.HasOne<AppUser>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
  b.HasOne<CustomerOrder>().WithMany().HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);
 }
}
