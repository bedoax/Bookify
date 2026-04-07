using Bookify.Models.DTO.Notification;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResponse<PaginatedResult<NotificationDto>>> GetNotificationsAsync(int userId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<ServiceResponse<bool>> MarkAsReadAsync(int userId, int notificationId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteNotificationAsync(int userId, int notificationId, CancellationToken ct = default);
    }
}
