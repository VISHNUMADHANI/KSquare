using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class CustomJewelryRequestMedia : AuditableEntity
{
    private CustomJewelryRequestMedia() { }
    public CustomJewelryRequestMedia(Guid requestId, string objectKey, string contentType, long contentLength, int displayOrder)
    {
        CustomJewelryRequestId = requestId; ObjectKey = objectKey; ContentType = contentType; ContentLength = contentLength; DisplayOrder = displayOrder;
    }
    public Guid CustomJewelryRequestId { get; private set; }
    public CustomJewelryRequest Request { get; private set; } = null!;
    public string ObjectKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long ContentLength { get; private set; }
    public int DisplayOrder { get; private set; }
}
