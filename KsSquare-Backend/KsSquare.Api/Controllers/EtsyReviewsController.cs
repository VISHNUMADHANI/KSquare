using Microsoft.AspNetCore.Mvc;
namespace KsSquare.Api.Controllers;
[ApiController, Route("api/etsy/reviews")]
public sealed class EtsyReviewsController(EtsyReviewsService reviews) : ControllerBase
{
    [HttpGet]
    public Task<EtsyReviewFeed> Get(CancellationToken ct) => reviews.GetAsync(ct);
}
