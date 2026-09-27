using KsSquare.Application.Abstractions.Authentication;
using KsSquare.Domain.Entities;

namespace KsSquare.Api.Services;

public sealed class DevelopmentOtpEmailSender(ILogger<DevelopmentOtpEmailSender> logger, IHostEnvironment environment) : IOtpEmailSender
{
    public Task SendAsync(string email, string fullName, string code, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("A production email provider must be configured before sending OTPs.");
        logger.LogWarning("DEVELOPMENT OTP for {Email} ({Purpose}): {Code}. Expires in 5 minutes.", email, purpose, code);
        return Task.CompletedTask;
    }
}
