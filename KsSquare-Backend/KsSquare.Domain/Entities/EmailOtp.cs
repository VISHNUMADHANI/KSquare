using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class EmailOtp : AuditableEntity
{
    private EmailOtp() { }
    public EmailOtp(Guid userId, OtpPurpose purpose, string codeHash, DateTimeOffset expiresAt)
    { UserId = userId; Purpose = purpose; CodeHash = codeHash; ExpiresAt = expiresAt.ToUniversalTime(); }

    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public OtpPurpose Purpose { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public bool IsUsable(DateTimeOffset now) => ConsumedAt is null && ExpiresAt > now && FailedAttempts < 5;
    public void RecordFailure() => FailedAttempts++;
    public void Consume(DateTimeOffset now) => ConsumedAt = now.ToUniversalTime();
    public void Invalidate(DateTimeOffset now) { if (ConsumedAt is null) ConsumedAt = now.ToUniversalTime(); }
}
