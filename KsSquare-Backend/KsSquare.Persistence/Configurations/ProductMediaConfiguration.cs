using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        builder.ToTable("product_media");
        builder.HasKey(media => media.Id);
        builder.Property(media => media.ObjectKey).HasMaxLength(500).IsRequired();
        builder.Property(media => media.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(media => media.AltText).HasMaxLength(250).IsRequired();
        builder.HasIndex(media => media.ObjectKey).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(media => new { media.ProductId, media.DisplayOrder });
        builder.HasOne(media => media.Product).WithMany(product => product.Media).HasForeignKey(media => media.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(media => media.DeletedAt == null);
    }
}
