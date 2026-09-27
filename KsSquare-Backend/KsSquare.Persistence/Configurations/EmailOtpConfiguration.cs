using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KsSquare.Persistence.Configurations;

public sealed class EmailOtpConfiguration : IEntityTypeConfiguration<EmailOtp>
{
    public void Configure(EntityTypeBuilder<EmailOtp> builder)
    {
        builder.ToTable("email_otps"); builder.HasKey(otp => otp.Id);
        builder.Property(otp => otp.Purpose).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(otp => otp.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasOne(otp => otp.User).WithMany().HasForeignKey(otp => otp.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(otp => new { otp.UserId, otp.Purpose, otp.CreatedAt });
    }
}
