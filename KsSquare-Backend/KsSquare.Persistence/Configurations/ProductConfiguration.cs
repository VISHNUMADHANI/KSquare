using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Name).HasMaxLength(180).IsRequired();
        builder.Property(product => product.Sku).HasMaxLength(64).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(4000).IsRequired();
        builder.Property(product => product.OriginalPrice).HasPrecision(18, 2);
        builder.Property(product => product.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(product => product.NamePricePerLetter).HasPrecision(18, 2);
        builder.Property(product => product.NameFixedPrice).HasPrecision(18, 2);
        builder.Ignore(product => product.FinalPrice);
        builder.HasIndex(product => product.Sku).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(product => new { product.CategoryId, product.IsActive, product.IsAvailable });
        builder.HasOne(product => product.Category).WithMany(category => category.Products).HasForeignKey(product => product.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(product => product.Options).WithMany(option => option.Products).UsingEntity<Dictionary<string, object>>(
            "ProductOptionAssignment",
            right => right.HasOne<ProductOption>().WithMany().HasForeignKey("ProductOptionId").OnDelete(DeleteBehavior.Cascade),
            left => left.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
            join => { join.ToTable("product_option_assignments"); join.HasKey("ProductId", "ProductOptionId"); });
        builder.HasMany(product => product.Subcategories).WithMany(category => category.SubcategoryProducts).UsingEntity<Dictionary<string, object>>(
            "ProductSubcategoryAssignment",
            right => right.HasOne<Category>().WithMany().HasForeignKey("SubcategoryId").OnDelete(DeleteBehavior.Restrict),
            left => left.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
            join => { join.ToTable("product_subcategory_assignments"); join.HasKey("ProductId", "SubcategoryId"); join.HasIndex("SubcategoryId"); });
        builder.HasQueryFilter(product => product.DeletedAt == null);
    }
}
