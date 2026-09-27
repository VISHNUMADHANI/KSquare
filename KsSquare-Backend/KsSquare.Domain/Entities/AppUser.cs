using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class AppUser : AuditableEntity
{
    private AppUser() { }

    public AppUser(string fullName, string email, string phone, string passwordHash, UserRole role)
    {
        FullName = fullName.Trim();
        Email = NormalizeEmail(email);
        Phone = phone.Trim();
        PasswordHash = passwordHash;
        Role = role;
    }

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }

    public void VerifyEmail() => IsEmailVerified = true;
    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;
    public void UpdateProfile(string fullName, string phone) { FullName = fullName.Trim(); Phone = phone.Trim(); }
    public void UpdatePendingRegistration(string fullName, string phone, string passwordHash)
    {
        if (IsEmailVerified) throw new InvalidOperationException("A verified account cannot be registered again.");
        FullName = fullName.Trim(); Phone = phone.Trim(); PasswordHash = passwordHash;
    }
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
