using Bookify.Data;
using Bookify.Models.DTO.Notification;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        public NotificationService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<bool>> DeleteNotificationAsync(int userId, int notificationId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid userId");

            if (notificationId <= 0)
                return ServiceResponse<bool>.Fail("Invalid userId");

            var notification = await GetNotificationAsync(userId, notificationId);

            if (notification == null)
                return ServiceResponse<bool>.Fail("Notification not found");

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, "Notification deleted");
        }


        public async Task<ServiceResponse<PaginatedResult<NotificationDto>>> GetNotificationsAsync(
            int userId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<PaginatedResult<NotificationDto>>.Fail("Invalid userId");

            if (pageNumber <= 0)
                return ServiceResponse<PaginatedResult<NotificationDto>>.Fail("Invalid pageNumber");

            if (pageSize <= 0)
                return ServiceResponse<PaginatedResult<NotificationDto>>.Fail("Invalid pageSize");

            var query = GetBaseQueryAsync(userId, ct);

            var totalCount = await query.CountAsync(ct);

            var data = await GetDataAsync(query, pageNumber, pageSize, ct);



            var result = new PaginatedResult<NotificationDto>
            {
                Items = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResponse<PaginatedResult<NotificationDto>>.Ok(result);
        }


        public async Task<ServiceResponse<bool>> MarkAsReadAsync(int userId, int notificationId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid userId");

            if (notificationId <= 0)
                return ServiceResponse<bool>.Fail("Invalid userId");

            var updated = await _context.Notifications
                .Where(n => n.Id == notificationId && n.UserId == userId && !n.ReadStatus)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadStatus, true), ct);

            if (updated == 0)
                return ServiceResponse<bool>.Fail("Notification not found or already read");

            return ServiceResponse<bool>.Ok(true, "Marked as read");
        }
        private IQueryable<Notification> GetBaseQueryAsync(int userId, CancellationToken ct = default)
        {
            return _context.Notifications
                .Where(n => n.UserId == userId)
                .AsNoTracking();
        }
        private async Task<List<NotificationDto>> GetDataAsync(IQueryable<Notification> query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    ReadStatus = n.ReadStatus,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(ct);
        }
        private async Task<Notification?> GetNotificationAsync(int userId, int notificationId, CancellationToken ct = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);
        }
    }
}
