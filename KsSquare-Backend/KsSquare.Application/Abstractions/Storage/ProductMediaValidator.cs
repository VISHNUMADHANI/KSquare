namespace KsSquare.Application.Abstractions.Storage;
public static class ProductMediaValidator
{
 public static string Validate(string type, long size)
 {
  if (type is "video/mp4" or "video/webm") {
   if(size <= 0 || size > 25L * 1024 * 1024) throw new StorageValidationException("Product videos must be between 1 byte and 25 MB.");
   return type == "video/mp4" ? ".mp4" : ".webm";
  }
  return StorageFileValidator.ValidateAndGetExtension(type, size);
 }
}
