using KsSquare.Application.Abstractions.Storage;
namespace KsSquare.Storage.Tests;

public sealed class StorageFileValidatorTests
{
    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    [InlineData("image/jpeg; charset=binary", ".jpg")]
    public void ValidateAndGetExtension_AcceptsSupportedImages(string contentType, string expected) =>
        Assert.Equal(expected, StorageFileValidator.ValidateAndGetExtension(contentType, 100));

    [Fact]
    public void ValidateAndGetExtension_RejectsOversizedFile() =>
        Assert.Throws<StorageValidationException>(() => StorageFileValidator.ValidateAndGetExtension("image/jpeg", StorageFileValidator.MaximumFileSizeBytes + 1));

    [Fact]
    public void ValidateAndGetExtension_RejectsUnsupportedContentType() =>
        Assert.Throws<StorageValidationException>(() => StorageFileValidator.ValidateAndGetExtension("image/svg+xml", 100));
}
