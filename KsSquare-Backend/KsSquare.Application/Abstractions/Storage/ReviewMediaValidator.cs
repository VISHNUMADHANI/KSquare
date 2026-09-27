namespace KsSquare.Application.Abstractions.Storage;
public static class ReviewMediaValidator
{
 public static string Validate(string type, long size)
 {
  var extension=type switch { "image/jpeg"=>".jpg", "image/png"=>".png", "image/webp"=>".webp", "video/mp4"=>".mp4", "video/webm"=>".webm", "video/quicktime"=>".mov", _=>throw new StorageValidationException("Use JPG, PNG, WebP, MP4, WebM or MOV files.") };
  if(size<=0 || size>(type.StartsWith("video/")?25L:5L)*1024*1024) throw new StorageValidationException("Photos must be under 5 MB and videos under 25 MB.");
  return extension;
 }
 public static bool Matches(string type, byte[] h) => h.Length>=12 && type switch
 {
  "image/jpeg"=>h[0]==255 && h[1]==216 && h[2]==255,
  "image/png"=>h.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}),
  "image/webp"=>System.Text.Encoding.ASCII.GetString(h,0,4)=="RIFF" && System.Text.Encoding.ASCII.GetString(h,8,4)=="WEBP",
  "video/mp4" or "video/quicktime"=>System.Text.Encoding.ASCII.GetString(h,4,4)=="ftyp",
  "video/webm"=>h[0]==26 && h[1]==69 && h[2]==223 && h[3]==163,
  _=>false
 };
}
