namespace KsSquare.Domain.Common;

public abstract class AuditableEntity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public void SetCreatedAt(DateTimeOffset timestamp) => CreatedAt = timestamp.ToUniversalTime();

    public void SetUpdatedAt(DateTimeOffset timestamp) => UpdatedAt = timestamp.ToUniversalTime();
}
