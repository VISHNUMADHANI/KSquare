namespace KsSquare.Application.Features.CustomJewelryRequests.DTOs;

public sealed record CustomRequestOptionDto(Guid Id, string Type, string Name, string? ImageUrl);
public sealed record CustomRequestMediaDto(Guid Id, string Url, string ContentType, long ContentLength, int DisplayOrder);
public sealed record CustomJewelryRequestDto(
    Guid Id, string RequestNumber, string CategoryName, string CustomerName, string Email, string Phone, string Description,
    string Status, decimal? FixedPrice, string Currency, IReadOnlyList<CustomRequestOptionDto> Options,
    IReadOnlyList<CustomRequestMediaDto> Media, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record CreateCustomJewelryRequestRequest(Guid CategoryId, IReadOnlyCollection<Guid>? OptionIds, string CustomerName, string Email, string Phone, string Description);
public sealed record CreateCustomJewelryRequestResult(CustomJewelryRequestDto Request, string AccessToken);
public sealed record SetCustomRequestQuoteRequest(decimal FixedPrice);
public sealed record UpdateCustomRequestStatusRequest(string Status);
public sealed record CustomRequestFileInput(Stream Content, string ContentType, long ContentLength);
