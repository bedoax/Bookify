using Bookify.Models.DTO.Registration;
using Bookify.Models.DTO.UserProfile;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResponse<RegistrationResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
        Task<ServiceResponse<UserProfileDto>> GetProfileAsync(int userId, CancellationToken ct = default);
        Task<ServiceResponse<UserProfileDto>> UpdateProfileAsync(int userId, UserProfileUpdateDto dto, CancellationToken ct = default);
    }
    
}

