using Bookify.Models.DTO.Auth;
using Bookify.Models.DTO.RefreshTokenDto;
using Bookify.Models.DTO.Registration;
using Bookify.Models.Mapping;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAccountService _accountService;

        public AuthController(
            IAuthenticationService authenticationService,
            IAccountService accountService)
        {
            _authenticationService = authenticationService;
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var result = await _accountService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return StatusCode(201, result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var result = await _authenticationService.LoginAsync(dto);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();

            var result = await _authenticationService.LogoutAsync(userId);

            if (!result.Success)
                return BadRequest(result.Message);

            return NoContent();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest("Invalid refresh token");

            var result = await _authenticationService.RefreshTokenAsync(dto.RefreshToken);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result.Data);
        }
    }
}
