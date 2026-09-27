using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(120).IsRequired();
        builder.Property(category => category.Slug).HasMaxLength(140).IsRequired();
        builder.Property(category => category.ImageObjectKey).HasMaxLength(512);
        builder.Property(category => category.ImageContentType).HasMaxLength(100);
        builder.HasIndex(category => category.Slug).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(category => new { category.ParentId, category.Name }).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasOne(category => category.Parent).WithMany(category => category.Children).HasForeignKey(category => category.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(category => category.DeletedAt == null);
    }
}
