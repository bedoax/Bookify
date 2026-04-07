using Bookify.Models.DTO.Account;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        /*
        POST /api/password/change
        POST /api/password/forgot
        POST /api/password/reset
         */
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        public PasswordController(IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordRequestDto dto)
        {
            var userId = User.GetUserId();
            var result = await _passwordRecoveryService.ChangePasswordAsync(userId, dto);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }
        [HttpPost("forget")]
        public async Task<IActionResult> ForgetPassword([FromBody]string email)
        {
            var result = await _passwordRecoveryService.SendForgotPasswordEmailAsync(email);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }
        // look at it again
        [HttpPut("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            var result = await _passwordRecoveryService.ResetPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }
    }
}
