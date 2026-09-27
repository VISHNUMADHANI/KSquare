using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class CustomJewelryRequestConfiguration : IEntityTypeConfiguration<CustomJewelryRequest>
{
    public void Configure(EntityTypeBuilder<CustomJewelryRequest> builder)
    {
        builder.ToTable("custom_jewelry_requests"); builder.HasKey(request => request.Id);
        builder.Property(request => request.RequestNumber).HasMaxLength(32).IsRequired(); builder.HasIndex(request => request.RequestNumber).IsUnique();
        builder.Property(request => request.AccessTokenHash).HasMaxLength(64).IsRequired();
        builder.Property(request => request.CustomerName).HasMaxLength(150).IsRequired(); builder.Property(request => request.Email).HasMaxLength(254).IsRequired(); builder.Property(request => request.Phone).HasMaxLength(32).IsRequired(); builder.Property(request => request.Description).HasMaxLength(4000).IsRequired(); builder.Property(request => request.FixedPrice).HasPrecision(18, 2);
        builder.HasOne(request => request.Category).WithMany().HasForeignKey(request => request.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(request => request.Options).WithMany().UsingEntity(join => join.ToTable("custom_jewelry_request_options"));
        builder.HasMany(request => request.Media).WithOne(media => media.Request).HasForeignKey(media => media.CustomJewelryRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(request => request.DeletedAt == null && request.Category.DeletedAt == null);
    }
}
