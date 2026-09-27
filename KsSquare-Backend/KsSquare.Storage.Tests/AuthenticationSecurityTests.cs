using KsSquare.Domain.Entities;
using KsSquare.Infrastructure.Authentication;

namespace KsSquare.Storage.Tests;

public sealed class AuthenticationSecurityTests
{
    [Fact]
    public void PasswordHash_RoundTripsWithoutContainingPassword()
    {
        var service = new Pbkdf2PasswordService();
        var hash = service.Hash("SecurePass123");
        Assert.DoesNotContain("SecurePass123", hash);
        Assert.True(service.Verify("SecurePass123", hash));
        Assert.False(service.Verify("WrongPass123", hash));
    }

    [Fact]
    public void NewCustomer_IsUnverifiedAndHasCustomerRole()
    {
        var user = new AppUser("Test Customer", "TEST@example.com", "+15555555555", "hash", UserRole.Customer);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.False(user.IsEmailVerified);
        user.VerifyEmail();
        Assert.True(user.IsEmailVerified);
    }

    [Fact]
    public void Otp_StopsBeingUsableAfterFiveFailures()
    {
        var otp = new EmailOtp(Guid.NewGuid(), OtpPurpose.VerifyEmail, "hash", DateTimeOffset.UtcNow.AddMinutes(5));
        for (var index = 0; index < 5; index++) otp.RecordFailure();
        Assert.False(otp.IsUsable(DateTimeOffset.UtcNow));
    }
}
