using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class ProductBraceletVariantConfiguration : IEntityTypeConfiguration<ProductBraceletVariant>
{
    public void Configure(EntityTypeBuilder<ProductBraceletVariant> builder)
    {
        builder.ToTable("product_bracelet_variants");
        builder.HasKey(variant => variant.Id);
        builder.Property(variant => variant.OriginalPrice).HasPrecision(18, 2);
        builder.Property(variant => variant.CombinationKey).HasMaxLength(100).IsRequired();
        builder.HasIndex(variant => new { variant.ProductId, variant.CombinationKey }).IsUnique();
        builder.HasOne(variant => variant.Product).WithMany(product => product.BraceletVariants).HasForeignKey(variant => variant.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(variant => variant.BraceletSizeOption).WithMany().HasForeignKey(variant => variant.BraceletSizeOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(variant => variant.BraceletStoneSizeOption).WithMany().HasForeignKey(variant => variant.BraceletStoneSizeOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(variant => variant.DeletedAt == null && variant.Product.DeletedAt == null && (variant.BraceletSizeOption == null || variant.BraceletSizeOption.DeletedAt == null) && (variant.BraceletStoneSizeOption == null || variant.BraceletStoneSizeOption.DeletedAt == null));
    }
}
