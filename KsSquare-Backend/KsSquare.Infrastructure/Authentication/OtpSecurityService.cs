using System.Security.Cryptography;
using System.Text;
using KsSquare.Application.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;

namespace KsSquare.Infrastructure.Authentication;

public sealed class OtpSecurityService : IOtpSecurityService
{
    private readonly byte[] pepper;
    public OtpSecurityService(IConfiguration configuration)
    {
        var value = configuration["AUTH_OTP_PEPPER"];
        if (string.IsNullOrWhiteSpace(value) || value.Length < 32) throw new InvalidOperationException("AUTH_OTP_PEPPER must contain at least 32 characters.");
        pepper = Encoding.UTF8.GetBytes(value);
    }
    public string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    public string HashCode(string code) { using var hmac = new HMACSHA256(pepper); return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(code))); }
    public bool VerifyCode(string code, string codeHash) { try { return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(HashCode(code)), Convert.FromHexString(codeHash)); } catch (FormatException) { return false; } }
}
