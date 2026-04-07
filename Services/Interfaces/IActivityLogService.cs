using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.UserProfile;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IActivityLogService
    {
        // Used internally by other services to log events silently
        Task<ServiceResponse<ActivityLogDto>> LogActivityAsync(int userId, int ActionTypeLogId, int? bookId = null, string? metadata = null, CancellationToken ct = default);
        Task<ServiceResponse<PaginatedResult<ActivityLogDto>>> GetActivityLogsAsync(int userId, PaginationDto dto, CancellationToken ct = default);
    }
}
