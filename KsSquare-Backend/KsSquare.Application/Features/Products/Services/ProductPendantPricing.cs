using KsSquare.Application.Features.Products.DTOs;
using KsSquare.Domain.Entities;
namespace KsSquare.Application.Features.Products.Services;
public sealed partial class ProductService
{
 private static void SetPendantVariants(Product product,IReadOnlyCollection<SaveProductPendantVariantRequest>? requested,string categoryName,bool active,bool available)
 {
  if(product.SupportsNamePersonalization){product.PendantVariants.Clear();return;}
  var variants=(requested??[]).ToArray();
  if(!CatalogCategoryNames.IsPendant(categoryName)){if(variants.Length>0)throw new CatalogValidationException("Size prices can be added only to pendant products.");product.PendantVariants.Clear();return;}
  var sizes=product.Options.Where(o=>o.Type==ProductOptionType.PendantSize).Select(o=>o.Id).ToHashSet();
  if(variants.Any(v=>!sizes.Contains(v.PendantSizeOptionId)))throw new CatalogValidationException("Each pendant price must use a size selected for this product.");
  if(variants.Any(v=>v.OriginalPrice<=0||decimal.Round(v.OriginalPrice,2)!=v.OriginalPrice))throw new CatalogValidationException("Every pendant size needs a positive price with at most two decimals.");
  if(variants.Select(v=>v.PendantSizeOptionId).Distinct().Count()!=variants.Length)throw new CatalogValidationException("Duplicate pendant size prices are not allowed.");
  if(active&&sizes.Count!=variants.Length)throw new CatalogValidationException("Enter a price for every selected pendant size, or remove sizes to use one price.");
  if(active&&available&&variants.Length>0&&variants.All(v=>!v.IsAvailable))throw new CatalogValidationException("At least one pendant size must be available.");
  var remaining=variants.ToList();foreach(var existing in product.PendantVariants.ToArray()){var row=remaining.FirstOrDefault(v=>v.PendantSizeOptionId==existing.PendantSizeOptionId);if(row is null)product.PendantVariants.Remove(existing);else{existing.Update(row.OriginalPrice,row.IsAvailable);remaining.Remove(row);}}
  foreach(var row in remaining)product.PendantVariants.Add(new(product.Id,row.PendantSizeOptionId,row.OriginalPrice,row.IsAvailable));
  if(variants.Length>0)product.SetStartingPrice(variants.Min(v=>v.OriginalPrice));
 }
}
