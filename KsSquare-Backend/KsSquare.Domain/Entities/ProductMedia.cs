using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class ProductMedia : AuditableEntity
{
    private ProductMedia() { }

    public ProductMedia(Guid productId, string objectKey, string contentType, long contentLength, string altText, int displayOrder)
    {
        ProductId = productId;
        ObjectKey = objectKey;
        ContentType = contentType;
        ContentLength = contentLength;
        AltText = altText.Trim();
        DisplayOrder = displayOrder;
    }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public string ObjectKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long ContentLength { get; private set; }
    public string AltText { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
}
