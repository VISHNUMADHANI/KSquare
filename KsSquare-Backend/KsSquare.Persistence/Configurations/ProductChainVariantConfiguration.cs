using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class ProductChainVariantConfiguration : IEntityTypeConfiguration<ProductChainVariant>
{
    public void Configure(EntityTypeBuilder<ProductChainVariant> builder)
    {
        builder.ToTable("product_chain_variants");
        builder.HasKey(variant => variant.Id);
        // Variant IDs are assigned by the domain, including new combinations on tracked products.
        builder.Property(variant => variant.Id).ValueGeneratedNever();
        builder.Property(variant => variant.OriginalPrice).HasPrecision(18, 2);
        builder.Property(variant => variant.CombinationKey).HasMaxLength(110).IsRequired();
        builder.HasIndex(variant => new { variant.ProductId, variant.CombinationKey }).IsUnique();
        builder.HasOne(variant => variant.Product).WithMany(product => product.ChainVariants).HasForeignKey(variant => variant.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(variant => variant.ChainSizeOption).WithMany().HasForeignKey(variant => variant.ChainSizeOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(variant => variant.ChainWidthOption).WithMany().HasForeignKey(variant => variant.ChainWidthOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(variant => variant.ChainDiamondSizeOption).WithMany().HasForeignKey(variant => variant.ChainDiamondSizeOptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(variant => variant.DeletedAt == null && variant.Product.DeletedAt == null && variant.ChainSizeOption.DeletedAt == null && (variant.ChainWidthOption == null || variant.ChainWidthOption.DeletedAt == null) && (variant.ChainDiamondSizeOption == null || variant.ChainDiamondSizeOption.DeletedAt == null));
    }
}
