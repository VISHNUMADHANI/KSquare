using System.Text.Json;
namespace KsSquare.Api;

public sealed record EtsyReviewCard(int Rating, string Text, string? ImageUrl, long CreatedAt, string ListingUrl);
public sealed record EtsyReviewFeed(string? ShopName, string? ShopUrl, EtsyReviewCard[] Items, DateTimeOffset? UpdatedAt, int? TotalReviews = null, double? AverageRating = null);

// Single shop, shared cache: browsing categories never triggers a request per visitor.
public sealed class EtsyReviewsService(IHttpClientFactory clients, IConfiguration config)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private EtsyReviewFeed cached = new(null, null, [], null);
    private DateTimeOffset nextRefresh;
    public async Task<EtsyReviewFeed> GetAsync(CancellationToken ct)
    {
        var key = config["ETSY_API_KEY"]?.Trim();
        var secret = config["ETSY_SHARED_SECRET"]?.Trim();
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret) || !long.TryParse(config["ETSY_SHOP_ID"], out var shopId) || shopId <= 0)
            return new(null, null, [], null);
        await gate.WaitAsync(ct);
        try
        {
            if (DateTimeOffset.UtcNow < nextRefresh) return FreshCache();
            using var http = clients.CreateClient("EtsyReviews");
            async Task<JsonDocument> Fetch(string path)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://openapi.etsy.com/v3/application/shops/" + shopId + path);
                request.Headers.Add("x-api-key", key + ":" + secret);
                using var response = await http.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();
                return JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            }
            try
            {
                using var shop = await Fetch("");
                using var reviews = await Fetch("/reviews?limit=100&offset=0");
                var name = shop.RootElement.GetProperty("shop_name").GetString();
                var cards = Parse(reviews.RootElement);
                int? total = reviews.RootElement.TryGetProperty("count", out var count) && count.TryGetInt32(out var n) && n >= 0 ? n : null;
                double? average = shop.RootElement.TryGetProperty("review_average", out var rating) && rating.TryGetDouble(out var a) && a >= 1 && a <= 5 ? a : null;
                cached = new(name, "https://www.etsy.com/shop/" + Uri.EscapeDataString(name ?? "") + "#reviews", cards, DateTimeOffset.UtcNow, total, average);
                nextRefresh = DateTimeOffset.UtcNow.AddHours(6);
            }
            catch (Exception e) when (e is HttpRequestException or JsonException or InvalidOperationException or KeyNotFoundException or TaskCanceledException)
            {
                // Never expose upstream errors/credentials, or hammer Etsy on rate limits.
                nextRefresh = DateTimeOffset.UtcNow.AddMinutes(5);
            }
            return FreshCache();
        }
        finally { gate.Release(); }
    }
    private EtsyReviewFeed FreshCache() => cached.UpdatedAt > DateTimeOffset.UtcNow.AddHours(-7) ? cached : new(null, null, [], null);
    public static EtsyReviewCard[] Parse(JsonElement root)
    {
        var cards = new List<EtsyReviewCard>();
        foreach (var r in root.GetProperty("results").EnumerateArray())
        {
            if (!r.TryGetProperty("rating", out var rating) || !rating.TryGetInt32(out var stars) || stars < 1 || stars > 5) continue;
            var text = r.TryGetProperty("review", out var body) && body.ValueKind == JsonValueKind.String ? body.GetString() ?? "" : "";
            var photo = r.TryGetProperty("image_url_fullxfull", out var image) && image.ValueKind == JsonValueKind.String ? image.GetString() : null;
            if (!Uri.TryCreate(photo, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) || !(uri.Host == "etsystatic.com" || uri.Host.EndsWith(".etsystatic.com", StringComparison.OrdinalIgnoreCase))) photo = null;
            var date = r.TryGetProperty("created_timestamp", out var timestamp) && timestamp.TryGetInt64(out var seconds) && seconds > 0 && seconds < 253402300800 ? seconds : 0;
            var listing = r.TryGetProperty("listing_id", out var id) && id.TryGetInt64(out var number) && number > 0 ? "https://www.etsy.com/listing/" + number : "https://www.etsy.com";
            cards.Add(new(stars, text, photo, date, listing));
        }
        return cards.OrderByDescending(r => r.CreatedAt).Take(100).ToArray();
    }
}
