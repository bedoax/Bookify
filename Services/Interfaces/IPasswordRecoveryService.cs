using Bookify.Models.DTO.Account;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IPasswordRecoveryService
    {
        Task<ServiceResponse<bool>> SendForgotPasswordEmailAsync(string email, CancellationToken ct = default);
        Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default);
    }
}