using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class Category : AuditableEntity
{
    private Category() { }

    public Category(string name, string slug, Guid? parentId, bool isActive, bool showInCustom = false)
    {
        Update(name, slug, parentId, isActive, showInCustom);
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public Category? Parent { get; private set; }
    public bool IsActive { get; private set; }
    public bool ShowInCustom { get; private set; }
    public string? ImageObjectKey { get; private set; }
    public string? ImageContentType { get; private set; }
    public long? ImageContentLength { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public ICollection<Product> SubcategoryProducts { get; private set; } = new List<Product>();

    public void Update(string name, string slug, Guid? parentId, bool isActive, bool showInCustom = false)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ParentId = parentId;
        IsActive = isActive;
        ShowInCustom = showInCustom;
    }

    public void SetImage(string objectKey, string contentType, long contentLength)
    {
        ImageObjectKey = objectKey;
        ImageContentType = contentType;
        ImageContentLength = contentLength;
    }

    public void RemoveImage()
    {
        ImageObjectKey = null;
        ImageContentType = null;
        ImageContentLength = null;
    }
}
