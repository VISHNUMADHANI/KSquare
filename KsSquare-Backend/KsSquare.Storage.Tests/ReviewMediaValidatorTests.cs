using KsSquare.Application.Abstractions.Storage;
namespace KsSquare.Storage.Tests;
public sealed class ReviewMediaValidatorTests {
 [Theory][InlineData("video/mp4", ".mp4")][InlineData("video/quicktime", ".mov")][InlineData("video/webm", ".webm")][InlineData("image/jpeg", ".jpg")]
 public void AcceptsSupportedFiles(string type,string ext)=>Assert.Equal(ext,ReviewMediaValidator.Validate(type,200));
 [Theory][InlineData("text/html",100)][InlineData("image/svg+xml",100)][InlineData("image/png",5242881)][InlineData("video/mp4",26214401)][InlineData("video/mp4",0)][InlineData("video/quicktime",26214401)]
 public void RejectsUnsafeAndOversizeFiles(string type,long length)=>Assert.Throws<StorageValidationException>(()=>ReviewMediaValidator.Validate(type,length));
 [Fact]public void RejectsSpoofedImage()=>Assert.False(ReviewMediaValidator.Matches("image/png",System.Text.Encoding.ASCII.GetBytes("<html>unsafe</html>")));
 [Theory][InlineData("video/mp4")][InlineData("video/quicktime")]public void AcceptsCameraVideoHeader(string type)=>Assert.True(ReviewMediaValidator.Matches(type,new byte[]{0,0,0,24,102,116,121,112,105,115,111,109}));
 [Fact]public void OtherUploadFlowsStillRejectVideo()=>Assert.Throws<StorageValidationException>(()=>StorageFileValidator.ValidateAndGetExtension("video/mp4",200));
}
