namespace KsSquare.Application.Features.Authentication;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto> VerifyEmailAsync(VerifyOtpRequest request, CancellationToken cancellationToken);
    Task ResendVerificationAsync(string email, CancellationToken cancellationToken);
    Task<AuthUserDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<AuthUserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken cancellationToken);
}
