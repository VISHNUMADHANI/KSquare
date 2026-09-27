using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Application.Features.CustomJewelryRequests.DTOs;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;

namespace KsSquare.Application.Features.CustomJewelryRequests.Services;

public sealed class CustomJewelryRequestService(ICustomJewelryRequestRepository repository, IObjectStorageService storage) : ICustomJewelryRequestService
{
    public async Task<CreateCustomJewelryRequestResult> CreateAsync(CreateCustomJewelryRequestRequest request, IReadOnlyList<CustomRequestFileInput> files, CancellationToken cancellationToken = default)
    {
        ValidateContact(request);
        if (files.Count is < 1 or > 6) throw new CatalogValidationException("Upload between 1 and 6 reference images.");
        var category = await repository.GetCategoryAsync(request.CategoryId, cancellationToken);
        if (category is null || !category.IsActive || !category.ShowInCustom || category.ParentId.HasValue) throw new CatalogValidationException("Select an active Custom category.");
        var optionIds = (request.OptionIds ?? Array.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToArray();
        var options = await repository.GetOptionsAsync(optionIds, cancellationToken);
        if (options.Count != optionIds.Length || options.Any(option => !option.IsActive || !AllowedTypes(category.Name).Contains(option.Type))) throw new CatalogValidationException("One or more selected options do not apply to this Custom category.");
        foreach (var file in files) StorageFileValidator.ValidateAndGetExtension(file.ContentType, file.ContentLength);

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var customRequest = new CustomJewelryRequest($"KSQ-CUST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..25].ToUpperInvariant(), Hash(token), category.Id, request.CustomerName, request.Email, request.Phone, request.Description);
        foreach (var option in options) customRequest.Options.Add(option);
        var uploaded = new List<string>();
        try
        {
            for (var index = 0; index < files.Count; index++)
            {
                var file = files[index]; var extension = StorageFileValidator.ValidateAndGetExtension(file.ContentType, file.ContentLength); var key = ObjectKeyBuilder.ForCustomRequest(customRequest.Id, extension);
                var result = await storage.UploadAsync(new ObjectUploadRequest(file.Content, key, file.ContentType, file.ContentLength, StorageAccess.Private), cancellationToken);
                uploaded.Add(result.ObjectKey); customRequest.Media.Add(new CustomJewelryRequestMedia(customRequest.Id, result.ObjectKey, result.ContentType, result.ContentLength, index));
            }
            repository.Add(customRequest); await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var key in uploaded) try { await storage.DeleteAsync(key, StorageAccess.Private, CancellationToken.None); } catch { }
            throw;
        }
        return new CreateCustomJewelryRequestResult(Map(customRequest), token);
    }

    public async Task<CustomJewelryRequestDto?> GetCustomerAsync(Guid id, string token, CancellationToken cancellationToken = default)
    {
        var request = await repository.GetByIdAsync(id, cancellationToken);
        return request is null || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(request.AccessTokenHash), Encoding.ASCII.GetBytes(Hash(token ?? string.Empty))) ? null : Map(request);
    }
    public async Task<IReadOnlyList<CustomJewelryRequestDto>> GetAdminAllAsync(CancellationToken cancellationToken = default) => (await repository.GetAllAsync(cancellationToken)).Select(Map).ToArray();
    public async Task<CustomJewelryRequestDto?> GetAdminAsync(Guid id, CancellationToken cancellationToken = default) { var request = await repository.GetByIdAsync(id, cancellationToken); return request is null ? null : Map(request); }
    public async Task<CustomJewelryRequestDto?> SetQuoteAsync(Guid id, decimal fixedPrice, CancellationToken cancellationToken = default) { if (fixedPrice <= 0) throw new CatalogValidationException("Fixed price must be greater than zero."); var request = await repository.GetByIdAsync(id, cancellationToken); if (request is null) return null; try { request.SetQuote(fixedPrice); } catch (InvalidOperationException ex) { throw new CatalogValidationException(ex.Message); } await repository.SaveChangesAsync(cancellationToken); return Map(request); }
    public async Task<CustomJewelryRequestDto?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default) { var request = await repository.GetByIdAsync(id, cancellationToken); if (request is null) return null; try { if (status.Equals("Contacted", StringComparison.OrdinalIgnoreCase)) request.MarkContacted(); else if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) request.Cancel(); else throw new CatalogValidationException("Status must be Contacted or Cancelled."); } catch (InvalidOperationException ex) { throw new CatalogValidationException(ex.Message); } await repository.SaveChangesAsync(cancellationToken); return Map(request); }

    private CustomJewelryRequestDto Map(CustomJewelryRequest request) => new(request.Id, request.RequestNumber, request.Category.Name, request.CustomerName, request.Email, request.Phone, request.Description, request.Status.ToString(), request.FixedPrice, "USD", request.Options.OrderBy(option => option.Type).ThenBy(option => option.DisplayOrder).Select(option => new CustomRequestOptionDto(option.Id, option.Type.ToString(), option.Name, string.IsNullOrWhiteSpace(option.ImageObjectKey) ? null : storage.GetPublicUrl(option.ImageObjectKey))).ToArray(), request.Media.OrderBy(media => media.DisplayOrder).Select(media => new CustomRequestMediaDto(media.Id, storage.GetSignedUrl(media.ObjectKey, TimeSpan.FromHours(1)), media.ContentType, media.ContentLength, media.DisplayOrder)).ToArray(), request.CreatedAt, request.UpdatedAt);
    private static void ValidateContact(CreateCustomJewelryRequestRequest request) { if (string.IsNullOrWhiteSpace(request.CustomerName) || request.CustomerName.Trim().Length > 150) throw new CatalogValidationException("Name is required and cannot exceed 150 characters."); try { _ = new MailAddress(request.Email); } catch { throw new CatalogValidationException("Enter a valid email address."); } if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Trim().Length is < 7 or > 32) throw new CatalogValidationException("Enter a valid phone number."); if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 4000) throw new CatalogValidationException("Description is required and cannot exceed 4000 characters."); }
    private static ProductOptionType[] AllowedTypes(string categoryName) { var name = categoryName.ToLowerInvariant(); return name.Contains("grill") ? [ProductOptionType.Color] : name.Contains("brace") ? [ProductOptionType.BraceletSize, ProductOptionType.BraceletStoneSize, ProductOptionType.Color] : name.Contains("ring") ? [ProductOptionType.RingSize, ProductOptionType.Color] : CatalogCategoryNames.IsPendant(categoryName.Replace("+", " ")) ? (name.Contains("chain") || name.Contains("set") ? [ProductOptionType.CustomPendantStyle, ProductOptionType.PendantSize, ProductOptionType.Color, ProductOptionType.ChainSize, ProductOptionType.ChainWidth, ProductOptionType.ChainDiamondSize] : [ProductOptionType.CustomPendantStyle, ProductOptionType.PendantSize, ProductOptionType.Color]) : name.Contains("chain") ? [ProductOptionType.ChainSize, ProductOptionType.ChainWidth, ProductOptionType.ChainDiamondSize, ProductOptionType.Color] : []; }
    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
