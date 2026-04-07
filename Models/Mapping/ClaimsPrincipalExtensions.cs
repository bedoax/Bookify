using System.Security.Claims;

namespace Bookify.Models.Mapping
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // الـ GlobalExceptionHandlingMiddleware بتاعنا هيمسك دي ويرجع 401
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User identification claim is missing or invalid.");
            }

        
            if (!int.TryParse(userIdClaim, out var userId))
            {
                // لو الـ ID في التوكن مش رقم (مثلاً GUID)، بنرمي Exception تانية
                throw new ArgumentException("User ID in claims is not a valid integer.");
            }

            return userId;
        }
    }
}