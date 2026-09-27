using KsSquare.Domain.Common;
namespace KsSquare.Domain.Entities;
public sealed class ProductPendantVariant : AuditableEntity
{
 private ProductPendantVariant() {}
 public ProductPendantVariant(Guid productId, Guid pendantSizeOptionId, decimal originalPrice, bool isAvailable){ProductId=productId;PendantSizeOptionId=pendantSizeOptionId;Update(originalPrice,isAvailable);}
 public Guid ProductId { get; private set; }
 public Product Product { get; private set; } = null!;
 public Guid PendantSizeOptionId { get; private set; }
 public ProductOption PendantSizeOption { get; private set; } = null!;
 public decimal OriginalPrice { get; private set; }
 public bool IsAvailable { get; private set; }
 public void Update(decimal price,bool available){OriginalPrice=price;IsAvailable=available;}
}
