namespace KsSquare.Application.Features.Authentication;

public sealed record RegisterRequest(string FullName, string Email, string Phone, string Password);
public sealed record VerifyOtpRequest(string Email, string Code);
public sealed record LoginRequest(string Email, string Password);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Code, string NewPassword);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record UpdateProfileRequest(string FullName, string Phone);
public sealed record BootstrapAdminRequest(string FullName, string Email, string Phone, string Password);
public sealed record AuthUserDto(Guid Id, string FullName, string Email, string Phone, string Role, bool IsEmailVerified);
public sealed record AuthMessageDto(string Message);
