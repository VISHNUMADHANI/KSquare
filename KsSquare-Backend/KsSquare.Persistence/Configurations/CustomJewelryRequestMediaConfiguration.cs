using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class CustomJewelryRequestMediaConfiguration : IEntityTypeConfiguration<CustomJewelryRequestMedia>
{
    public void Configure(EntityTypeBuilder<CustomJewelryRequestMedia> builder)
    {
        builder.ToTable("custom_jewelry_request_media"); builder.HasKey(media => media.Id); builder.Property(media => media.ObjectKey).HasMaxLength(500).IsRequired(); builder.Property(media => media.ContentType).HasMaxLength(100).IsRequired(); builder.HasIndex(media => media.ObjectKey).IsUnique(); builder.HasQueryFilter(media => media.DeletedAt == null && media.Request.DeletedAt == null);
    }
}
