using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KsSquare.Application.Features.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous, HttpPost("register")]
    public async Task<ActionResult<AuthMessageDto>> Register(RegisterRequest request, CancellationToken cancellationToken)
    { await auth.RegisterAsync(request, cancellationToken); return Accepted(new AuthMessageDto("Registration received. Enter the OTP sent to your email.")); }

    [AllowAnonymous, HttpPost("verify-email")]
    public async Task<ActionResult<AuthUserDto>> VerifyEmail(VerifyOtpRequest request, CancellationToken cancellationToken) => Ok(await auth.VerifyEmailAsync(request, cancellationToken));

    [AllowAnonymous, HttpPost("resend-verification")]
    public async Task<ActionResult<AuthMessageDto>> ResendVerification(ForgotPasswordRequest request, CancellationToken cancellationToken)
    { await auth.ResendVerificationAsync(request.Email, cancellationToken); return Ok(new AuthMessageDto("A new verification OTP was generated.")); }

    [AllowAnonymous, HttpPost("login")]
    public async Task<ActionResult<AuthUserDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    { var user = await auth.LoginAsync(request, cancellationToken); await SignInAsync(user); return Ok(user); }

    [AllowAnonymous, HttpPost("forgot-password")]
    public async Task<ActionResult<AuthMessageDto>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    { await auth.ForgotPasswordAsync(request, cancellationToken); return Ok(new AuthMessageDto("If a verified account exists, a reset OTP was generated.")); }

    [AllowAnonymous, HttpPost("reset-password")]
    public async Task<ActionResult<AuthMessageDto>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    { await auth.ResetPasswordAsync(request, cancellationToken); return Ok(new AuthMessageDto("Password reset successfully.")); }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return NoContent(); }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<AuthUserDto>> Me(CancellationToken cancellationToken) => Ok(await auth.GetUserAsync(UserId(), cancellationToken));

    [Authorize(Roles = "Customer"), HttpPut("profile")]
    public async Task<ActionResult<AuthUserDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken) => Ok(await auth.UpdateProfileAsync(UserId(), request, cancellationToken));

    [Authorize, HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    { await auth.ChangePasswordAsync(UserId(), request, cancellationToken); return NoContent(); }

    [AllowAnonymous, HttpPost("admin/bootstrap")]
    public async Task<ActionResult<AuthUserDto>> BootstrapAdmin([FromHeader(Name = "X-Admin-Setup-Key")] string? setupKey, BootstrapAdminRequest request, CancellationToken cancellationToken)
    {
        var expected = configuration["ADMIN_SETUP_KEY"];
        if (string.IsNullOrWhiteSpace(expected) || expected.Length < 32) return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Admin bootstrap is not configured." });
        if (string.IsNullOrEmpty(setupKey) || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(setupKey), Encoding.UTF8.GetBytes(expected))) return Unauthorized(new { error = "Invalid Admin setup key." });
        var user = await auth.BootstrapAdminAsync(request, cancellationToken); return StatusCode(StatusCodes.Status201Created, user);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new AuthUnauthorizedException("Invalid authentication session.");
    private Task SignInAsync(AuthUserDto user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true, AllowRefresh = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
    }
}
