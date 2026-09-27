using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KsSquare.Persistence.Configurations;
public sealed class ProductPendantVariantConfiguration : IEntityTypeConfiguration<ProductPendantVariant>
{
 public void Configure(EntityTypeBuilder<ProductPendantVariant> b){b.ToTable("product_pendant_variants");b.HasKey(x=>x.Id);b.Property(x=>x.OriginalPrice).HasPrecision(18,2);b.HasIndex(x=>new{x.ProductId,x.PendantSizeOptionId}).IsUnique();b.HasOne(x=>x.Product).WithMany(x=>x.PendantVariants).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Cascade);b.HasOne(x=>x.PendantSizeOption).WithMany().HasForeignKey(x=>x.PendantSizeOptionId).OnDelete(DeleteBehavior.Restrict);b.HasQueryFilter(x=>x.DeletedAt==null&&x.Product.DeletedAt==null&&x.PendantSizeOption.DeletedAt==null);}
}
