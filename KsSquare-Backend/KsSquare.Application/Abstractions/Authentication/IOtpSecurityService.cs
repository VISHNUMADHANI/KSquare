namespace KsSquare.Application.Abstractions.Authentication;

public interface IOtpSecurityService
{
    string GenerateCode();
    string HashCode(string code);
    bool VerifyCode(string code, string codeHash);
}
