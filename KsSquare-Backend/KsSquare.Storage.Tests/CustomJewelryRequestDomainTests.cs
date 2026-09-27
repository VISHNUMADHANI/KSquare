using KsSquare.Domain.Entities;

namespace KsSquare.Storage.Tests;

public sealed class CustomJewelryRequestDomainTests
{
    [Fact]
    public void NewRequest_StartsAsSubmittedWithoutPrice()
    {
        var request = Create();
        Assert.Equal(CustomJewelryRequestStatus.Submitted, request.Status);
        Assert.Null(request.FixedPrice);
    }

    [Fact]
    public void Quote_SetsFixedPriceAndQuotedStatus()
    {
        var request = Create();
        request.MarkContacted(); request.SetQuote(725.50m);
        Assert.Equal(CustomJewelryRequestStatus.Quoted, request.Status);
        Assert.Equal(725.50m, request.FixedPrice);
    }

    [Fact]
    public void QuotedRequest_CannotReturnToContacted()
    {
        var request = Create();
        request.SetQuote(725.50m);

        var action = () => request.MarkContacted();

        Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(CustomJewelryRequestStatus.Quoted, request.Status);
        Assert.Equal(725.50m, request.FixedPrice);
    }

    [Fact]
    public void CancelledRequest_CannotBeQuoted()
    {
        var request = Create(); request.Cancel();
        Assert.Throws<InvalidOperationException>(() => request.SetQuote(500));
    }

    private static CustomJewelryRequest Create() => new("KSQ-CUST-TEST", "hash", Guid.NewGuid(), "Customer", "customer@example.com", "+15555555555", "Custom ring idea");
}
