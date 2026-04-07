using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }
    /*
     Don't forget to add those endPonts and create thier services 
    GET /api/analytics/tags/most-searched
    GET /api/analytics/revenue
    GET /api/analytics/growth
     */
    
    [HttpGet("books/top-rated-books")]
    public async Task<IActionResult> GetTopRatedBooks([FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetTopRatedBooksAsync(limit);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }
    [HttpGet("books/most-favorited")]
    public async Task<IActionResult> GetMostFavoritedBooks([FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetMostFavoritedBooksAsync(limit);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("books/most-read")]
    public async Task<IActionResult> GetMostReadBooks([FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetMostReadBooksAsync(limit);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("books/most-listened")]
    public async Task<IActionResult> GetMostListenedAudioBooks([FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetMostListenedAudioBooksAsync(limit);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("books/most-reviewed")]
    public async Task<IActionResult> GetBooksWithMostReviews([FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetBooksWithMostReviewsAsync(limit);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("users/most-active")]
    public async Task<IActionResult> GetMostActiveUsers([FromQuery] int limit = 10)
    {
        var result = await _analyticsService.GetMostActiveUsersAsync(limit);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptionMetrics()
    {
        var result = await _analyticsService.GetSubscriptionMetricsAsync();
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }
    // this cost 3 requests per one time
    // there three solution on database side like using stored procedure or view or matrilized view 
    // there cach way to use it 3 request one time , and if it changed u will change the caching 
    // using real time singlr to fetch in real time 
    // or using Task.whenall to wait until all get reasult , it reduce letancy a little bit , but not recommended 
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var topBooks = await _analyticsService.GetMostReadBooksAsync(5);
        var users = await _analyticsService.GetMostActiveUsersAsync(5);
        var subs = await _analyticsService.GetSubscriptionMetricsAsync();

        return Ok(new
        {
            TopBooks = topBooks.Data,
            TopUsers = users.Data,
            Subscriptions = subs.Data
        });
    }
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats([FromQuery] int limit = 5)
    {
        // result هنا هو من نوع ServiceResponse<DashboardDataDto>
        var result = await _analyticsService.GetDashboardDataAsyncByDapper(limit);

        if (!result.Success) 
        {
            return BadRequest(result.Message); 
        }

        return Ok(result.Data); 
    }
}
