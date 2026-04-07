using Bookify.Models.DTO.UserProfile;
using Bookify.Models.Mapping;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public UsersController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.GetUserId();

            var result = await _accountService.GetProfileAsync(userId);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var userId = User.GetUserId();

            var result = await _accountService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
