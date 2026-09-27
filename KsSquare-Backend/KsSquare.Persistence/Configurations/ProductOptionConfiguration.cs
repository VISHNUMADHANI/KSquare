using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.ToTable("product_options");
        builder.HasKey(option => option.Id);
        builder.Property(option => option.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(option => option.Name).HasMaxLength(100).IsRequired();
        builder.Property(option => option.SizeGuideObjectKey).HasMaxLength(512);
        builder.Property(option => option.ImageObjectKey).HasMaxLength(512);
        builder.Property(option => option.ImageContentType).HasMaxLength(100);
        builder.HasIndex(option => new { option.Type, option.Name }).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(option => new { option.Type, option.IsActive, option.DisplayOrder });
        builder.HasQueryFilter(option => option.DeletedAt == null);
    }
}
