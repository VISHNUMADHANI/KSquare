using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class ProductOption : AuditableEntity
{
    private ProductOption() { }

    public ProductOption(ProductOptionType type, string name, int displayOrder, bool isActive)
    {
        Update(type, name, displayOrder, isActive);
    }

    public ProductOptionType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? SizeGuideObjectKey { get; private set; }
    public void SetSizeGuide(string? key) => SizeGuideObjectKey = key;
    public string? ImageObjectKey { get; private set; }
    public string? ImageContentType { get; private set; }
    public long? ImageContentLength { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public void Update(ProductOptionType type, string name, int displayOrder, bool isActive)
    {
        Type = type;
        Name = name.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void SetImage(string objectKey, string contentType, long contentLength)
    {
        ImageObjectKey = objectKey; ImageContentType = contentType; ImageContentLength = contentLength;
    }

    public void RemoveImage()
    {
        ImageObjectKey = null; ImageContentType = null; ImageContentLength = null;
    }
}
