using KsSquare.Domain.Entities;

namespace KsSquare.Application.Abstractions.Authentication;

public interface IOtpEmailSender
{
    Task SendAsync(string email, string fullName, string code, OtpPurpose purpose, CancellationToken cancellationToken);
}
