using KsSquare.Domain.Entities;

namespace KsSquare.Storage.Tests;

public sealed class CatalogDomainTests
{
    [Theory]
    [InlineData(100, 15, 85)]
    [InlineData(99.99, 12.5, 87.49)]
    [InlineData(250, 0, 250)]
    public void Product_CalculatesUsdSellingPrice(decimal originalPrice, decimal discount, decimal expected)
    {
        var product = new Product("Name Pendant", "pen-nam-001", string.Empty, originalPrice, discount, Guid.NewGuid(), true, true);
        Assert.Equal(expected, product.FinalPrice);
    }

    [Fact]
    public void Product_NormalizesEditableSku()
    {
        var product = new Product("Ring", " ring-gold-001 ", string.Empty, 500, 0, Guid.NewGuid(), true, true);
        Assert.Equal("RING-GOLD-001", product.Sku);
    }

    [Fact]
    public void Category_SupportsParentCategory()
    {
        var parentId = Guid.NewGuid();
        var category = new Category("Gold", "gold", parentId, true);
        Assert.Equal(parentId, category.ParentId);
    }
}
