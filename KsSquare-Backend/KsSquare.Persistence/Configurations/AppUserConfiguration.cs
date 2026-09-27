using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users"); builder.HasKey(user => user.Id);
        builder.Property(user => user.FullName).HasMaxLength(150).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(254).IsRequired(); builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.Phone).HasMaxLength(32).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(user => user.Role);
    }
}
