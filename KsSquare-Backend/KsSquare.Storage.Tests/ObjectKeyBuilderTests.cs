using KsSquare.Application.Abstractions.Storage;
namespace KsSquare.Storage.Tests;

public sealed class ObjectKeyBuilderTests
{
    [Fact]
    public void ForProduct_BuildsExpectedKey()
    {
        var productId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var objectId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        Assert.Equal("products/11111111-1111-1111-1111-111111111111/thumbnail/22222222-2222-2222-2222-222222222222.webp", ObjectKeyBuilder.ForProduct(productId, "Thumbnail", "webp", objectId));
    }

    [Fact]
    public void ForCustomRequest_BuildsPrivateKey()
    {
        var requestId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var objectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        Assert.Equal("custom-requests/33333333-3333-3333-3333-333333333333/44444444-4444-4444-4444-444444444444.jpg", ObjectKeyBuilder.ForCustomRequest(requestId, ".jpg", objectId));
    }
}
