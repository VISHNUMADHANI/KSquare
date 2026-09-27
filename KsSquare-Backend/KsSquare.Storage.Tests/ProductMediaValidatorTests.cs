using KsSquare.Application.Abstractions.Storage;
using Xunit;
public class ProductMediaValidatorTests
{
 [Theory]
 [InlineData("video/mp4", ".mp4")]
 [InlineData("video/webm", ".webm")]
 public void AcceptsProductVideos(string type, string extension) => Assert.Equal(extension, ProductMediaValidator.Validate(type, 25L*1024*1024));
 [Theory]
 [InlineData("video/mp4", 26214401)]
 [InlineData("video/mp4", 0)]
 [InlineData("image/png", 10485761)]
 [InlineData("application/javascript", 100)]
 public void RejectsInvalidFiles(string type, long size) => Assert.Throws<StorageValidationException>(() => ProductMediaValidator.Validate(type, size));
 [Fact] public void PreservesPhotoLimit() => Assert.Equal(".png", ProductMediaValidator.Validate("image/png",10L*1024*1024));
}
