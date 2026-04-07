using Bookify.Data;
using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.DashBoard;
using Bookify.Models.DTO.Subscription;
using Bookify.Models.DTO.Tag;
using Bookify.Models.DTO.UserProfile;
using Bookify.Services.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Bookify.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AppDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetMostFavoritedBooksAsync(int limit, CancellationToken ct = default)
        {
            // get most favorited books by counting the number of times each book appears in users' favorites and order by count desc, then take the top 'limit' books

            if (limit < 0)
                return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Fail("Limit must be a non-negative integer");
            string metricLabel = "Favorites";

            var favoritedBooks = GetMostFavoriteBooksAsync(limit, ct, metricLabel);

            return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Ok(await favoritedBooks, "Most favorited books retrieved successfully");
        }
        public async Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetMostReadBooksAsync(int limit, CancellationToken ct = default)
        {
            if (limit < 0)
                return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Fail("Limit must be a non-negative integer");

            string metricLabel = "Reads";

            var mostReadedBooks = await GetMostReadBooksDataAsync(limit, ct, metricLabel);

            return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Ok(mostReadedBooks, "Most read books retrieved successfully");
        }
        public async Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetMostListenedAudioBooksAsync(int limit, CancellationToken ct = default)
        {

            if (limit < 0)
                return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Fail("Limit must be a non-negative integer");
            string metricLabel = "Listens";
            var mostListenedAudioBooks = await GetMostListenBooksDataAsync(limit, ct, metricLabel);
            return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Ok(mostListenedAudioBooks, "Most listened audio books retrieved successfully");
        }
        public async Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetBooksWithMostReviewsAsync(int limit, CancellationToken ct = default)
        {
            if (limit < 0)
                return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Fail("Limit must be a non-negative integer");
            string metricLabel = "Reviews";
            var mostReviewedBooks = await GetBooksWithMostReviewsDataAsync(limit, ct, metricLabel);
            return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Ok(mostReviewedBooks, "Books with most reviews retrieved successfully");
        }
        /*public async Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetTopRatedBooksAsync(int limit, CancellationToken ct)
        {
            if (limit < 0)
                return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Fail("Limit must be a non-negative integer");

            string metricLabel = "Average Rating";

            var topRatedBooks = await GetTopRatedBooksDataAsync(limit, ct, metricLabel);

            return ServiceResponse<IEnumerable<TopBookMetricsDto>>.Ok(topRatedBooks, "Top rated books retrieved successfully");
        }*/
        public async Task<ServiceResponse<IEnumerable<UserActivityMetricsDto>>> GetMostActiveUsersAsync(int limit, CancellationToken ct = default)
        {
            if (limit < 0)
                return ServiceResponse<IEnumerable<UserActivityMetricsDto>>.Fail("Limit must be a non-negative integer");

            var mostActiveUsersPerDays = await GetMostActiveUsersDataAsync(limit, ct);


            return ServiceResponse<IEnumerable<UserActivityMetricsDto>>.Ok(mostActiveUsersPerDays, "Most active users retrieved successfully");
        }
        public Task<ServiceResponse<IEnumerable<TagMetricsDto>>> GetMostSearchedTagsAsync(int limit, CancellationToken ct = default)
        {
            // TODO: Implement once search-log infrastructure is in place.
            // This should count tag appearances in search logs and return
            // the top 'limit' tags ordered by frequency.
            return Task.FromResult(
                ServiceResponse<IEnumerable<TagMetricsDto>>.Ok(
                    Enumerable.Empty<TagMetricsDto>(),
                    "Most-searched-tags analytics not yet implemented."));
        }
        public async Task<ServiceResponse<IEnumerable<SubscriptionMetricsDto>>> GetSubscriptionMetricsAsync(CancellationToken ct = default)
        {
            var subscriptionMetrics = await GetSubscriptionMetricsDataAsync(ct);

            return ServiceResponse<IEnumerable<SubscriptionMetricsDto>>.Ok(subscriptionMetrics, "Subscription metrics retrieved successfully");
        }
        public async Task<ServiceResponse<IEnumerable<TopBookRatedDto>>> GetTopRatedBooksAsync(int limit, CancellationToken ct = default)
        {
            if (limit <= 0 || limit > 100)
            {
                return ServiceResponse<IEnumerable<TopBookRatedDto>>.Fail("Limit must be between 1 and 100.");
            }

            var topBooks = await FetchTopRatedBooks(limit, ct);

            return new ServiceResponse<IEnumerable<TopBookRatedDto>>
            {
                Data = topBooks,
                Success = true,
                Message = "Top rated books retrieved successfully."
            };
        }
        // أضفنا الـ CancellationToken هنا
        public async Task<ServiceResponse<DashboardDataDto>> GetDashboardDataAsyncByDapper(int limit, CancellationToken ct = default)
        {
            if (limit <= 0)
                return ServiceResponse<DashboardDataDto>.Fail("Limit should be a positive number.");

            try
            {
                var connection = _context.Database.GetDbConnection();

                // تمرير الـ CancellationToken في الـ Open
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(ct);

                // استخدام CommandDefinition لتمرير الـ CancellationToken للـ Query نفسها
                var command = new CommandDefinition(
                    "GetDashboardData",
                    new { Limit = limit },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                using (var multi = await connection.QueryMultipleAsync(command))
                {
                    var dashboardData = new DashboardDataDto
                    {
                        TopBooks = (await multi.ReadAsync<TopBookRatedDto>()).ToList(),
                        TopUsers = (await multi.ReadAsync<UserActivityMetricsDto>()).ToList(),
                        Subscriptions = (await multi.ReadAsync<SubscriptionMetricsDto>()).ToList()
                    };

                    return ServiceResponse<DashboardDataDto>.Ok(dashboardData, "Dashboard data fetched successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard data via Dapper.");
                return ServiceResponse<DashboardDataDto>.Fail($"Error fetching dashboard data: {ex.Message}");
            }
        }

        private async Task<IEnumerable<TopBookMetricsDto>> GetMostFavoriteBooksAsync(int limit, CancellationToken ct, string metricLabel)
        {
            return await _context.FavoriteBooks
                  .AsNoTracking()
                  .Where(fb => fb.Book.DeletedAt == null)
                  .GroupBy(fb => new
                  {
                      fb.BookId,
                      fb.Book.Title,
                      fb.Book.CoverImageUrl
                  })
                  .Select(g => new TopBookMetricsDto
                  {
                      BookId = g.Key.BookId,
                      Title = g.Key.Title,
                      CoverImageUrl = g.Key.CoverImageUrl,
                      Count = g.Count(),
                      MetricLabel = metricLabel
                  })
                  .OrderByDescending(x => x.Count)
                  .Take(limit)
                  .ToListAsync(ct);
        }
        private async Task<IEnumerable<TopBookMetricsDto>> GetMostReadBooksDataAsync(int limit, CancellationToken ct, string metricLabel)
        {
            return await _context.UserBookProgress
                      .AsNoTracking()
                      .Where(rp => rp.Book.DeletedAt == null)
                      .GroupBy(rp => new
                      {
                          rp.BookId,
                          rp.Book.Title,
                          rp.Book.CoverImageUrl
                      })
                      .Select(g => new TopBookMetricsDto
                      {
                          BookId = g.Key.BookId,
                          Title = g.Key.Title,
                          CoverImageUrl = g.Key.CoverImageUrl,
                          Count = g.Select(x => x.UserId).Distinct().Count(),
                          MetricLabel = metricLabel
                      })
                      .OrderByDescending(x => x.Count)
                      .Take(limit)
                      .ToListAsync(ct);
        }
        private async Task<IEnumerable<TopBookMetricsDto>> GetMostListenBooksDataAsync(int limit, CancellationToken ct, string metricLabel)
        {
            return await _context.UserAudioBookProgress
                          .AsNoTracking()
                          .Where(rp => rp.Book.DeletedAt == null)
                          .GroupBy(rp => new
                          {
                              rp.BookId,
                              rp.Book.Title,
                              rp.Book.CoverImageUrl
                          })
                          .Select(g => new TopBookMetricsDto
                          {
                              BookId = g.Key.BookId,
                              Title = g.Key.Title,
                              CoverImageUrl = g.Key.CoverImageUrl,
                              Count = g.Select(x => x.UserId).Distinct().Count(),
                              MetricLabel = metricLabel
                          })
                          .OrderByDescending(x => x.Count)
                          .Take(limit)
                          .ToListAsync(ct);
        }
        private async Task<IEnumerable<TopBookMetricsDto>> GetBooksWithMostReviewsDataAsync(int limit, CancellationToken ct, string metricLabel)
        {
            return await _context
                .BookReview
                .AsNoTracking()
                .Where(bf => bf.Book.DeletedAt == null)
                .GroupBy(bf => new
                {
                    bf.BookId,
                    bf.Book.Title,
                    bf.Book.CoverImageUrl
                })
                 .Select(g => new TopBookMetricsDto
                 {
                     BookId = g.Key.BookId,
                     Title = g.Key.Title,
                     CoverImageUrl = g.Key.CoverImageUrl,
                     Count = g.Count(),
                     MetricLabel = metricLabel
                 }).OrderByDescending(x => x.Count)
                 .Take(limit)
                 .ToListAsync(ct);
        }
        /*private async Task<IEnumerable<TopBookMetricsDto>> GetTopRatedBooksDataAsync(int limit, CancellationToken ct, string metricLabel)
        {
            return await _context
                .BookFeedback
                .AsNoTracking()
                .Where(bf => bf.Book.DeletedAt == null)
                .GroupBy(bf => new
                {
                    bf.BookId,
                    bf.Book.Title,
                    bf.Book.CoverImageUrl
                })
                 .Select(g => new TopBookMetricsDto
                 {
                     BookId = g.Key.BookId,
                     Title = g.Key.Title,
                     CoverImageUrl = g.Key.CoverImageUrl,
                     Count = (int)g.Average(x => x.Rating), // Average rating rounded down to nearest integer
                     MetricLabel = metricLabel
                 }).OrderByDescending(x => x.Count)
                 .Take(limit)
                 .ToListAsync(ct);
        }*/
        private async Task<IEnumerable<UserActivityMetricsDto>> GetMostActiveUsersDataAsync(int limit, CancellationToken ct)
        {
            // if not working well then use include user to get the user data and
            // then group by user id and username and date to get the number of distinct days each user was active,
            // then order by count desc and take the top 'limit' users
            return await _context.ActivityLogs
                .AsNoTracking()
                .Where(a => a.User.DeletedAt == null)
                .GroupBy(a => new
                {
                    a.UserId,
                    a.User.Username,
                    Date = a.CreatedAt.Date
                })
                .Select(g => new
                {
                    g.Key.UserId,
                    UserName = g.Key.Username
                })
                .GroupBy(x => new
                {
                    x.UserId,
                    x.UserName
                })
                .Select(g => new UserActivityMetricsDto
                {
                    IdUser = g.Key.UserId,
                    UserName = g.Key.UserName,
                    NumberOfDaysActived = g.Count()
                })
                .OrderByDescending(x => x.NumberOfDaysActived)
                .Take(limit)
                .ToListAsync(ct);
        }
        private async Task<IEnumerable<SubscriptionMetricsDto>> GetSubscriptionMetricsDataAsync(CancellationToken ct)
        {
            // should check the trail and active status of user subscription 
            // get the subscription name from subscrption plan 
            // and group by subscription name and count number of user for each subscription 
            return await _context.UserSubscriptions
                .AsNoTracking()
                .Include(us => us.Plan)
                .Where(us => us.User.DeletedAt == null && !us.IsTrial && us.Status == "active")
                .GroupBy(us => new
                {
                    us.PlanId,
                    us.Plan.Name
                })
                .Select(g => new SubscriptionMetricsDto
                {
                    SubscriptionId = g.Key.PlanId,
                    SubscriptionName = g.Key.Name,
                    NumberOfSubscriptionsOfUser = g.Select(x => x.UserId).Distinct().Count()
                })
                .OrderByDescending(x => x.NumberOfSubscriptionsOfUser)
                .ToListAsync(ct);
        }
        private async Task<IEnumerable<TopBookRatedDto>> FetchTopRatedBooks(int limit, CancellationToken ct)
        {
            return await _context.Books
                 .AsNoTracking()
                 .Where(b => b.Feedbacks.Any() && b.DeletedAt == null)
                 .Select(b => new TopBookRatedDto
                 {
                     Id = b.Id,
                     Name = b.Title,
                     AverageRating = b.Feedbacks.Any() ? Math.Round(b.Feedbacks.Average(f => (double)f.Rating), 2) : 0,
                     TotalRatings = b.Feedbacks.Count()
                 })
                 .OrderByDescending(b => b.AverageRating)
                 .ThenByDescending(b => b.TotalRatings)
                 .Take(limit)
                 .ToListAsync(ct);
        }
        /*        private async Task<IEnumerable<TagMetricsDto>> GetMostSearchedTagsDataAsync(int limit, CancellationToken ct)
                {
                    // get the most searched tags by counting the number of times each tag appears in search logs and order by count desc, then take the top 'limit' tags
                    // here tags let use some id like 2 , and it group by  
                    return 
                }*/
    }
}
