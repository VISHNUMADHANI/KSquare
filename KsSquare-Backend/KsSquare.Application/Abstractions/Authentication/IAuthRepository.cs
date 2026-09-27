using KsSquare.Domain.Entities;

namespace KsSquare.Application.Abstractions.Authentication;

public interface IAuthRepository
{
    Task<AppUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<AppUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AdminExistsAsync(CancellationToken cancellationToken);
    Task<EmailOtp?> GetLatestOtpAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailOtp>> GetOpenOtpsAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken);
    void AddUser(AppUser user);
    void AddOtp(EmailOtp otp);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
