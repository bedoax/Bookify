using Bookify.Models.DTO.Auth;
using Bookify.Models.DTO.Author;
using Bookify.Services.Implementations;
using Microsoft.AspNetCore.Identity.Data;

namespace Bookify.Services.Interfaces
{

    public interface IAuthenticationService
    {
        Task<ServiceResponse<AuthResultDto>> LoginAsync(LoginDto loginDto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> LogoutAsync(int userId, CancellationToken ct = default);
        Task<ServiceResponse<AuthResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    }
}
