using KsSquare.Application.Features.Orders;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;

namespace KsSquare.Storage.Tests;

public sealed class OrderTests
{
    private static Product Product(bool available = true, bool personalized = false) => new("Test piece", "test", "", 100, 10, Guid.NewGuid(), available, true, personalized, 100, 15);
    [Fact] public void PricesQuantityUsingDiscountedCatalogAmount()
    {
        var p = Product(); var result = OrderPricing.Price(p, new(p.Id, 3, [], null));
        Assert.Equal(90, result.UnitPrice); Assert.Equal(270, result.Total);
    }
    [Theory] [InlineData(0)] [InlineData(-1)] [InlineData(21)]
    public void RejectsInvalidQuantity(int quantity) { var p = Product(); Assert.Throws<CatalogValidationException>(() => OrderPricing.Price(p, new(p.Id, quantity, [], null))); }
    [Fact] public void RejectsUnavailableProduct() { var p = Product(false); Assert.Throws<CatalogValidationException>(() => OrderPricing.Price(p, new(p.Id, 1, [], null))); }
    [Theory] [InlineData("")] [InlineData("NINECHARS")] [InlineData("A B")] [InlineData("AB!")] [InlineData("AB-12")] [InlineData("AB1234567")] [InlineData("AB\n")]
    public void RejectsInvalidPersonalization(string name) { var p = Product(personalized:true); Assert.Throws<CatalogValidationException>(() => OrderPricing.Price(p, new(p.Id, 1, [], name))); }
    [Fact] public void AcceptsEightLetterNameAndPreservesNote()
    {
        var p=Product(personalized:true);var result=OrderPricing.Price(p,new(p.Id,1,[],"ABCDEFGH","  Use a script font.  "));
        Assert.Equal(184.5m,result.UnitPrice);Assert.Equal("Use a script font.",result.PersonalizationNote);
        var copy=System.Text.Json.JsonSerializer.Deserialize<OrderItemDto>(System.Text.Json.JsonSerializer.Serialize(result));
        Assert.Equal(result.PersonalizationNote,copy!.PersonalizationNote);
    }
    [Theory] [InlineData("A",90)] [InlineData("ABCDE",90)] [InlineData("ABCDEF",103.5)] [InlineData("ABCDEFG",117)] [InlineData("ABCDEFGH",130.5)] [InlineData("Alex1",90)] [InlineData("Alex12",103.5)] [InlineData("AB123456",130.5)] [InlineData("2026",90)]
    public void IncludedLettersOnlyChargeExtrasThenDiscount(string name, decimal expected)
    { var p=Product(personalized:true);p.SetIncludedNameLetters(5);Assert.Equal(expected,OrderPricing.Price(p,new(p.Id,1,[],name)).UnitPrice); }
    [Fact] public void OrdinaryProductsAcceptNotesWithoutChangingPrice()
    {
        var p=Product();var result=OrderPricing.Price(p,new(p.Id,1,[],null,"Gift for Alex"));
        Assert.Equal(90,result.UnitPrice);Assert.Equal("Gift for Alex",result.PersonalizationNote);
        Assert.Throws<CatalogValidationException>(()=>OrderPricing.Price(p,new(p.Id,1,[],null,new string('x',1001))));
    }
    [Fact] public void PersonalizationUsesFixedPlusPerLetterPrice()
    {
        var p = Product(personalized:true); var result = OrderPricing.Price(p, new(p.Id, 2, [], "AB"));
        Assert.Equal(103.5m, result.UnitPrice); Assert.Equal(207m, result.Total); Assert.Contains("Name: AB", result.Details);
    }
    [Fact] public void ChainUsesSelectedVariantAndRejectsMissingOrForeignOptions()
    {
        var p = Product(); var size = new ProductOption(ProductOptionType.ChainSize,"20 inch",0,true); p.Options.Add(size);
        p.ChainVariants.Add(new(p.Id,size.Id,null,null,250,true));
        Assert.Equal(225,OrderPricing.Price(p,new(p.Id,1,[size.Id],null)).UnitPrice);
        Assert.Throws<CatalogValidationException>(()=>OrderPricing.Price(p,new(p.Id,1,[],null)));
        Assert.Throws<CatalogValidationException>(()=>OrderPricing.Price(p,new(p.Id,1,[Guid.NewGuid()],null)));
        Assert.Throws<CatalogValidationException>(()=>OrderPricing.Price(p,new(p.Id,1,[size.Id,size.Id],null)));
    }
    [Fact] public void BraceletChecksAvailability()
    {
        var p=Product();var size=new ProductOption(ProductOptionType.BraceletSize,"7 inch",0,true);p.Options.Add(size);
        p.BraceletVariants.Add(new(p.Id,size.Id,null,300,false));
        Assert.Throws<CatalogValidationException>(()=>OrderPricing.Price(p,new(p.Id,1,[size.Id],null)));
    }
    [Fact] public void OrdersRemainExplicitlyDemoAndFollowFulfillmentSequence()
    {
        var order=new CustomerOrder(Guid.NewGuid(),Guid.NewGuid(),"hash","Tester","test@example.test","1234567890","123 Test Street, Test City, USA","[]",100);
        Assert.True(order.IsDemo);Assert.Equal("DemoPaid",order.PaymentStatus);Assert.StartsWith("DEMO-",order.PaymentReference);
        Assert.Throws<InvalidOperationException>(()=>order.ChangeStatus("Delivered"));
        order.ChangeStatus("Processing");order.ChangeStatus("Shipped");order.ChangeStatus("Delivered");
        Assert.Equal(3,order.Version);Assert.Throws<InvalidOperationException>(()=>order.ChangeStatus("Cancelled"));
    }
    [Fact] public void CancellationIsTerminal()
    {
        var order=new CustomerOrder(Guid.NewGuid(),Guid.NewGuid(),"hash","Tester","test@example.test","1234567890","123 Test Street, Test City, USA","[]",100);
        order.ChangeStatus("Cancelled");Assert.Throws<InvalidOperationException>(()=>order.ChangeStatus("Processing"));
    }
}
