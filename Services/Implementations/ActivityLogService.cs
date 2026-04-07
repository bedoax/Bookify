using Bookify.Data;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.UserProfile;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;
        public ActivityLogService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<ActivityLogDto>> LogActivityAsync(int userId, int actionTypeLogId, int? bookId = null, string? metadata = null, CancellationToken ct = default)
        {
            var validationResult = ValidateActivityLog(userId, actionTypeLogId, bookId, metadata);
            if (!validationResult.Success)
                return ServiceResponse<ActivityLogDto>.Fail(validationResult.Message);

            var actionType = await _context.ActionTypes.FindAsync(new object[] { actionTypeLogId }, ct);
            if (actionType is null)
                return ServiceResponse<ActivityLogDto>.Fail("Action type not found.");

            var activityLog = CreateActivityLog(userId, actionTypeLogId, bookId, metadata, actionType);

            await _context.ActivityLogs.AddAsync(activityLog, ct);
            await _context.SaveChangesAsync(ct);

            string? bookTitle = null;
            if (bookId.HasValue)
            {
                bookTitle = await _context.Books
                    .Where(b => b.Id == bookId.Value)
                    .Select(b => b.Title)
                    .FirstOrDefaultAsync(ct);
            }

            return ServiceResponse<ActivityLogDto>.Ok(new ActivityLogDto
            {
                ActivityCode = actionType.Code,
                ActivityDescription = actionType.Description,
                BookTitle = bookTitle,
                Metadata = metadata
            }, "Activity logged successfully.");
        }
        public async Task<ServiceResponse<PaginatedResult<ActivityLogDto>>> GetActivityLogsAsync(int userId, PaginationDto dto, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<PaginatedResult<ActivityLogDto>>.Fail("Invalid user ID.");

            dto.PageNumber = dto.PageNumber < 1 ? 1 : dto.PageNumber;
            dto.PageSize = dto.PageSize <= 0 ? 10 : Math.Min(dto.PageSize, 100);

            var query = GetActivityLogsQuery(userId);

            var totalCount = await query.CountAsync(ct);
            var logDtos = await GetActivityLogDtosAsync(
                query.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize),
                ct);

            var paginatedResult = new PaginatedResult<ActivityLogDto>
            {
                Items = logDtos,
                TotalCount = totalCount,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize
            };

            return ServiceResponse<PaginatedResult<ActivityLogDto>>.Ok(
                paginatedResult,
                "Activity logs retrieved successfully.");
        }
        private ActivityLog CreateActivityLog(int userId, int actionTypeId, int? bookId, string? metadata, ActionType actionType)
        {
            return new ActivityLog
            {
                UserId = userId,
                ActionTypeId = actionTypeId,
                BookId = bookId,
                Metadata = metadata,
                ActionType = actionType,
                CreatedAt = DateTime.UtcNow
            };
        }
        private static ServiceResponse<bool> ValidateActivityLog(int userId, int ActionTypeLogId, int? bookId, string? metadata = null)
        {
            if (userId <= 0)
            {
                return ServiceResponse<bool>.Fail("userId data should be greater than 0.");
            }
            if (ActionTypeLogId <= 0)
            {
                return ServiceResponse<bool>.Fail("ActionTypeLogId data should be greater than 0.");
            }

            return ServiceResponse<bool>.Ok(true);
        }

        private async Task<ActivityLogDto?> GetActivityLog(int userId)
        {
            return await _context.ActivityLogs.Where(log => log.UserId == userId).Include(x => x.ActionType).Select(log => new ActivityLogDto
            {
                ActivityCode = log.ActionType.Code,
                ActivityDescription = log.ActionType.Description,
                BookTitle = log.Book.Title ?? null,
                Metadata = log.Metadata ?? null,
            }).FirstOrDefaultAsync();
        }
        private IQueryable<ActivityLog> GetActivityLogsQuery(int userId)
        {
            return _context.ActivityLogs
                .AsNoTracking()
                .Where(log => log.UserId == userId)
                .Include(log => log.ActionType)
                .Include(log => log.Book);
        }
        private async Task<List<ActivityLogDto>> GetActivityLogDtosAsync(IQueryable<ActivityLog> query, CancellationToken ct)
        {
            return await query
                .OrderByDescending(log => log.CreatedAt)
                .Select(log => new ActivityLogDto
                {
                    ActivityCode = log.ActionType.Code,
                    ActivityDescription = log.ActionType.Description,
                    BookTitle = log.Book == null ? null : log.Book.Title,
                    Metadata = log.Metadata
                })
                .ToListAsync(ct);
        }
    }
}
