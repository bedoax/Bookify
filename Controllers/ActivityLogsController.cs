using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.UserProfile;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetUserLogs([FromQuery] PaginationDto dto,CancellationToken ct = default)
        {
            var requestingUserId = User.GetUserId();
            var result = await _activityLogService.GetActivityLogsAsync(requestingUserId, dto, ct);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Note: Activities are often logged internally by other services, 
        // but if you need a direct endpoint:
        [HttpPost]
        public async Task<IActionResult> LogActivity(
            [FromBody] ActivityLogRequest request,
            CancellationToken ct = default)
        {
            var userId = User.GetUserId();
            var result = await _activityLogService.LogActivityAsync(
                userId,
                request.ActionTypeLogId,
                request.BookId,
                request.Metadata,
                ct);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        
        [Authorize(Roles = AppRoles.Admin)]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLogs(
            [FromQuery] int userId,
            [FromQuery] PaginationDto dto,
            CancellationToken ct = default)
        {
            var result = await _activityLogService.GetActivityLogsAsync(userId, dto, ct);
            return Ok(result);
        }
    }

    // Supporting Request DTO for the Post method
    public record ActivityLogRequest(int ActionTypeLogId, int? BookId, string? Metadata);
}