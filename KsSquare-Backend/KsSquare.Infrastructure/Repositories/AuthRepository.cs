using KsSquare.Application.Abstractions.Authentication;
using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace KsSquare.Infrastructure.Repositories;

public sealed class AuthRepository(AppDbContext context) : IAuthRepository
{
    public Task<AppUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken) => context.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    public Task<AppUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken) => context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    public Task<bool> AdminExistsAsync(CancellationToken cancellationToken) => context.Users.AnyAsync(user => user.Role == UserRole.Admin, cancellationToken);
    public Task<EmailOtp?> GetLatestOtpAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken) => context.EmailOtps.Where(otp => otp.UserId == userId && otp.Purpose == purpose && otp.ConsumedAt == null).OrderByDescending(otp => otp.CreatedAt).FirstOrDefaultAsync(cancellationToken);
    public async Task<IReadOnlyList<EmailOtp>> GetOpenOtpsAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken) => await context.EmailOtps.Where(otp => otp.UserId == userId && otp.Purpose == purpose && otp.ConsumedAt == null).ToListAsync(cancellationToken);
    public void AddUser(AppUser user) => context.Users.Add(user);
    public void AddOtp(EmailOtp otp) => context.EmailOtps.Add(otp);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
