using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.Subscription;
using Bookify.Models.DTO.UserProfile;

namespace Bookify.Models.DTO.DashBoard
{
    public class DashboardDataDto
    {
        public List<TopBookRatedDto> TopBooks { get; set; } = new();
        public List<UserActivityMetricsDto> TopUsers { get; set; } = new();
        public List<SubscriptionMetricsDto> Subscriptions { get; set; } = new();
    }
}
