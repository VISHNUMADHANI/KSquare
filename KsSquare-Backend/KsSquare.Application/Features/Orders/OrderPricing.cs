using System.Text.RegularExpressions;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;
namespace KsSquare.Application.Features.Orders;

public static class OrderPricing
{
    public static OrderItemDto Price(Product product, OrderLineRequest line)
    {
        if (!product.IsActive || !product.IsAvailable || product.DeletedAt != null)
            throw new CatalogValidationException($"{product.Name} is no longer available.");
        if (line.Quantity is < 1 or > 20) throw new CatalogValidationException("Choose a quantity between 1 and 20.");
        if ((line.PersonalizationNote?.Length ?? 0) > 1000) throw new CatalogValidationException("Personalization notes cannot exceed 1,000 characters.");
        var note = string.IsNullOrWhiteSpace(line.PersonalizationNote) ? null : line.PersonalizationNote.Trim();
        var ids = line.OptionIds ?? [];
        if (ids.Length > 20 || ids.Distinct().Count() != ids.Length) throw new CatalogValidationException("Invalid product options.");
        var applicableOptions = product.Options.Where(o => !product.SupportsNamePersonalization || o.Type != ProductOptionType.PendantSize);
        var selected = applicableOptions.Where(o => ids.Contains(o.Id) && o.IsActive && o.DeletedAt == null).ToArray();
        if (selected.Length != ids.Length || selected.GroupBy(o => o.Type).Any(g => g.Count() != 1)
            || applicableOptions.Select(o => o.Type).Distinct().Any(type => !selected.Any(o => o.Type == type)))
            throw new CatalogValidationException($"Choose valid options for {product.Name}.");
        Guid? Option(ProductOptionType type) => selected.FirstOrDefault(o => o.Type == type)?.Id;
        var price = product.FinalPrice;
        if (product.ChainVariants.Count > 0)
        {
            var variant = product.ChainVariants.FirstOrDefault(v => v.ChainSizeOptionId == Option(ProductOptionType.ChainSize)
                && v.ChainWidthOptionId == Option(ProductOptionType.ChainWidth) && v.ChainDiamondSizeOptionId == Option(ProductOptionType.ChainDiamondSize));
            if (variant is null || !variant.IsAvailable || variant.DeletedAt != null) throw new CatalogValidationException("This chain combination is unavailable.");
            price = Discount(variant.OriginalPrice, product.DiscountPercentage);
        }
        if (product.BraceletVariants.Count > 0)
        {
            var variant = product.BraceletVariants.FirstOrDefault(v => v.BraceletSizeOptionId == Option(ProductOptionType.BraceletSize)
                && v.BraceletStoneSizeOptionId == Option(ProductOptionType.BraceletStoneSize));
            if (variant is null || !variant.IsAvailable || variant.DeletedAt != null) throw new CatalogValidationException("This bracelet combination is unavailable.");
            price = Discount(variant.OriginalPrice, product.DiscountPercentage);
        }
        var pendant = product.PendantVariants.FirstOrDefault(v=>v.PendantSizeOptionId==Option(ProductOptionType.PendantSize));
        if (!product.SupportsNamePersonalization && product.PendantVariants.Count > 0) {
            if(pendant is null || !pendant.IsAvailable || pendant.DeletedAt!=null) throw new CatalogValidationException("This pendant size is unavailable.");
            price = Discount(pendant.OriginalPrice, product.DiscountPercentage);
        }
        var details = selected.Select(o => o.Name).ToList();
        if (product.SupportsNamePersonalization)
        {
            if (!Regex.IsMatch(line.PersonalizedName ?? "", @"\A[A-Za-z0-9]{1,8}\z")) throw new CatalogValidationException("Enter 1 to 8 letters or numbers for the personalized name.");
            price = Discount(product.NameFixedPrice + Math.Max(0, line.PersonalizedName!.Length - product.IncludedNameLetters) * product.NamePricePerLetter, product.DiscountPercentage);
            details.Add("Name: " + line.PersonalizedName);
        }
        else if (!string.IsNullOrEmpty(line.PersonalizedName)) throw new CatalogValidationException("This product does not support a personalized name.");
        price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        if (price <= 0) throw new CatalogValidationException("This product does not have a valid price.");
        return new(product.Id, product.Name, line.Quantity, price, details.ToArray(), !product.SupportsNamePersonalization, product.SupportsNamePersonalization ? "Personalized name and letter pieces are final sale." : null, PersonalizationNote: note);
    }
    private static decimal Discount(decimal price, decimal percent) => decimal.Round(price * (100m - percent) / 100m, 2, MidpointRounding.AwayFromZero);
}
