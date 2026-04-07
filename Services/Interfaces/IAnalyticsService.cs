using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.DashBoard;
using Bookify.Models.DTO.Subscription;
using Bookify.Models.DTO.Tag;
using Bookify.Models.DTO.UserProfile;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetMostFavoritedBooksAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetMostReadBooksAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetMostListenedAudioBooksAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<TopBookMetricsDto>>> GetBooksWithMostReviewsAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<UserActivityMetricsDto>>> GetMostActiveUsersAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<TagMetricsDto>>> GetMostSearchedTagsAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<SubscriptionMetricsDto>>> GetSubscriptionMetricsAsync(CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<TopBookRatedDto>>> GetTopRatedBooksAsync(int limit, CancellationToken ct = default);
        Task<ServiceResponse<DashboardDataDto>> GetDashboardDataAsyncByDapper(int limit, CancellationToken ct = default);
    }

}
