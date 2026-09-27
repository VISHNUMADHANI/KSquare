using System.Net.Mail;
using KsSquare.Application.Abstractions.Authentication;
using KsSquare.Domain.Entities;

namespace KsSquare.Application.Features.Authentication;

public sealed class AuthService(IAuthRepository repository, IPasswordService passwords, IOtpSecurityService otpSecurity, IOtpEmailSender emailSender) : IAuthService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ValidateIdentity(request.FullName, request.Email, request.Phone); ValidatePassword(request.Password);
        var email = AppUser.NormalizeEmail(request.Email);
        var existing = await repository.GetUserByEmailAsync(email, cancellationToken);
        if (existing?.IsEmailVerified == true) throw new AuthConflictException("An account already exists for this email.");
        var passwordHash = passwords.Hash(request.Password);
        var user = existing ?? new AppUser(request.FullName, email, request.Phone, passwordHash, UserRole.Customer);
        if (existing is null) repository.AddUser(user); else user.UpdatePendingRegistration(request.FullName, request.Phone, passwordHash);
        var code = await AddOtpAsync(user, OtpPurpose.VerifyEmail, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await emailSender.SendAsync(user.Email, user.FullName, code, OtpPurpose.VerifyEmail, cancellationToken);
    }

    public async Task<AuthUserDto> VerifyEmailAsync(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var user = await RequiredUserByEmailAsync(request.Email, cancellationToken);
        if (user.IsEmailVerified) return Map(user);
        await ValidateOtpAsync(user, OtpPurpose.VerifyEmail, request.Code, cancellationToken);
        user.VerifyEmail(); await repository.SaveChangesAsync(cancellationToken); return Map(user);
    }

    public async Task ResendVerificationAsync(string email, CancellationToken cancellationToken)
    {
        var user = await RequiredUserByEmailAsync(email, cancellationToken);
        if (user.IsEmailVerified) throw new AuthConflictException("This email is already verified.");
        var code = await AddOtpAsync(user, OtpPurpose.VerifyEmail, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await emailSender.SendAsync(user.Email, user.FullName, code, OtpPurpose.VerifyEmail, cancellationToken);
    }

    public async Task<AuthUserDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByEmailAsync(AppUser.NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) throw new AuthUnauthorizedException("Invalid email or password.");
        if (!user.IsEmailVerified) throw new AuthUnauthorizedException("Verify your email before signing in.");
        return Map(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByEmailAsync(AppUser.NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !user.IsEmailVerified) return;
        var code = await AddOtpAsync(user, OtpPurpose.ResetPassword, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await emailSender.SendAsync(user.Email, user.FullName, code, OtpPurpose.ResetPassword, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        ValidatePassword(request.NewPassword); var user = await RequiredUserByEmailAsync(request.Email, cancellationToken);
        await ValidateOtpAsync(user, OtpPurpose.ResetPassword, request.Code, cancellationToken);
        user.ChangePassword(passwords.Hash(request.NewPassword)); await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthUserDto> GetUserAsync(Guid id, CancellationToken cancellationToken) => Map(await RequiredUserByIdAsync(id, cancellationToken));

    public async Task<AuthUserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length > 150) throw new AuthValidationException("Enter a valid full name.");
        ValidatePhone(request.Phone); var user = await RequiredUserByIdAsync(id, cancellationToken); user.UpdateProfile(request.FullName, request.Phone);
        await repository.SaveChangesAsync(cancellationToken); return Map(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        ValidatePassword(request.NewPassword); var user = await RequiredUserByIdAsync(id, cancellationToken);
        if (!passwords.Verify(request.CurrentPassword, user.PasswordHash)) throw new AuthUnauthorizedException("Current password is incorrect.");
        user.ChangePassword(passwords.Hash(request.NewPassword)); await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthUserDto> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken cancellationToken)
    {
        if (await repository.AdminExistsAsync(cancellationToken)) throw new AuthConflictException("The Admin account already exists.");
        ValidateIdentity(request.FullName, request.Email, request.Phone); ValidatePassword(request.Password);
        if (await repository.GetUserByEmailAsync(AppUser.NormalizeEmail(request.Email), cancellationToken) is not null) throw new AuthConflictException("An account already exists for this email.");
        var user = new AppUser(request.FullName, request.Email, request.Phone, passwords.Hash(request.Password), UserRole.Admin); user.VerifyEmail(); repository.AddUser(user);
        await repository.SaveChangesAsync(cancellationToken); return Map(user);
    }

    private async Task<string> AddOtpAsync(AppUser user, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow; foreach (var existing in await repository.GetOpenOtpsAsync(user.Id, purpose, cancellationToken)) existing.Invalidate(now);
        var code = otpSecurity.GenerateCode(); repository.AddOtp(new EmailOtp(user.Id, purpose, otpSecurity.HashCode(code), now.Add(OtpLifetime))); return code;
    }

    private async Task ValidateOtpAsync(AppUser user, OtpPurpose purpose, string code, CancellationToken cancellationToken)
    {
        if (code.Length != 6 || !code.All(char.IsDigit)) throw new AuthValidationException("Enter the 6-digit OTP.");
        var otp = await repository.GetLatestOtpAsync(user.Id, purpose, cancellationToken);
        if (otp is null || !otp.IsUsable(DateTimeOffset.UtcNow)) throw new AuthValidationException("The OTP is invalid or expired. Request a new code.");
        if (!otpSecurity.VerifyCode(code, otp.CodeHash)) { otp.RecordFailure(); await repository.SaveChangesAsync(cancellationToken); throw new AuthValidationException("The OTP is invalid or expired. Request a new code."); }
        otp.Consume(DateTimeOffset.UtcNow);
    }

    private async Task<AppUser> RequiredUserByEmailAsync(string email, CancellationToken cancellationToken) => await repository.GetUserByEmailAsync(AppUser.NormalizeEmail(email), cancellationToken) ?? throw new AuthValidationException("Account not found.");
    private async Task<AppUser> RequiredUserByIdAsync(Guid id, CancellationToken cancellationToken) => await repository.GetUserByIdAsync(id, cancellationToken) ?? throw new AuthUnauthorizedException("Account not found.");
    private static AuthUserDto Map(AppUser user) => new(user.Id, user.FullName, user.Email, user.Phone, user.Role.ToString(), user.IsEmailVerified);
    private static void ValidateIdentity(string name, string email, string phone) { if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 150) throw new AuthValidationException("Enter a valid full name."); if (!MailAddress.TryCreate(email, out _) || email.Length > 254) throw new AuthValidationException("Enter a valid email address."); ValidatePhone(phone); }
    private static void ValidatePhone(string phone) { var value = phone.Trim(); if (value.Length is < 7 or > 32) throw new AuthValidationException("Enter a valid phone number."); }
    private static void ValidatePassword(string password) { if (password.Length < 10 || password.Length > 128 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit)) throw new AuthValidationException("Password must be 10–128 characters and contain uppercase, lowercase and a number."); }
}
