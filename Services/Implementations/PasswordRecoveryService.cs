using Bookify.Data;
using Bookify.Models.DTO.Account;
using Bookify.Models.Entities;
using Bookify.Models.RestCode;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class PasswordRecoveryService : IPasswordRecoveryService
    {
        private readonly IPasswordService _passwordService;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public PasswordRecoveryService(AppDbContext context, IPasswordService passwordService, IEmailService emailService)
        {
            _context = context;
            _passwordService = passwordService;
            _emailService = emailService;
        }
        // maybe look at it again later 
        public async Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto dto, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid user ID.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null)
                return ServiceResponse<bool>.Fail("User not found.");

            var validatePassword = ValidateChangePassword(user, user.PasswordHash, dto);
            if (!validatePassword.Success)
                return ServiceResponse<bool>.Fail(validatePassword.Message);



            try
            {
                var activeCodes = await GetAllActiveCodesAsync(user, ct);
                DisableActiveCodes(activeCodes);

                user.PasswordHash = _passwordService.HashPassword(user, dto.NewPassword.Trim());

                await _context.SaveChangesAsync(ct);


                return ServiceResponse<bool>.Ok(true, "Password has been changed successfully.");
            }
            catch
            {

                throw;
            }


        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto dto, CancellationToken ct = default)
        {
            var validation = ValidateResetPassword(dto);
            if (!validation.Success) return ServiceResponse<bool>.Fail(validation.Message);

            var hashedInputCode = ResetCodeHasher.HashCode(dto.Code.Trim());

            var email = dto.Email.Trim().ToLower();

            var resetCode = await _context.PasswordResetCodes
                .Include(rc => rc.User)
                .Where(rc => rc.User.Email == email
                          && rc.CodeHash == hashedInputCode
                          && !rc.IsUsed
                          && rc.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(ct);

            if (resetCode == null)
                return ServiceResponse<bool>.Fail("Invalid or expired code.");

            using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var updated = await _context.PasswordResetCodes
                    .Where(x => x.Id == resetCode.Id && !x.IsUsed)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true), ct);

                if (updated == 0)
                    return ServiceResponse<bool>.Fail("Code already used.");

                resetCode.User.PasswordHash =
                    _passwordService.HashPassword(resetCode.User, dto.NewPassword.Trim());

                await _context.PasswordResetCodes
                    .Where(x => x.UserId == resetCode.UserId && !x.IsUsed)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true), ct);

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return ServiceResponse<bool>.Ok(true, "Password reset successfully.");
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

        }

        public async Task<ServiceResponse<bool>> SendForgotPasswordEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResponse<bool>.Fail("Email is required.");

            var user = await GetUserByEmailAsync(email, ct);

            // رسالة عامة لأسباب أمنية
            if (user == null)
                return ServiceResponse<bool>.Ok(true, "If the email exists, a reset code has been sent.");

            var oldCodes = await GetAllActiveCodesAsync(user, ct);
            DisableActiveCodes(oldCodes);

            var code = ResetCodeGenerator.GenerateCode(6);
            var codeHash = ResetCodeHasher.HashCode(code);

            var resetCode = new PasswordResetCode
            {
                UserId = user.Id,
                CodeHash = codeHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _context.PasswordResetCodes.AddAsync(resetCode, ct);
            await _context.SaveChangesAsync(ct);

            var subject = "Bookify Password Reset Code";
            var body = BuildResetPasswordEmailBody(code);

            await _emailService.SendAsync(user.Email, subject, body);

            return ServiceResponse<bool>.Ok(true, "If the email exists, a reset code has been sent.");
        }

        private ServiceResponse<bool> ValidateChangePassword(User user, string oldPasswordHash, ChangePasswordRequestDto dto)
        {
            if (dto == null)
                return ServiceResponse<bool>.Fail("Request data is required.");

            if (string.IsNullOrWhiteSpace(dto.OldPassword))
                return ServiceResponse<bool>.Fail("Old password is required.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return ServiceResponse<bool>.Fail("New password is required.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                return ServiceResponse<bool>.Fail("Confirm password is required.");

            if (dto.NewPassword.Trim() != dto.ConfirmPassword.Trim())
                return ServiceResponse<bool>.Fail("New password and confirm password do not match.");

            if (!_passwordService.VerifyPassword(user, oldPasswordHash, dto.OldPassword))
                return ServiceResponse<bool>.Fail("The old password is incorrect.");

            if (dto.NewPassword.Length < 8)
                return ServiceResponse<bool>.Fail("Weak password");
            return ServiceResponse<bool>.Ok(true);
        }

        private ServiceResponse<bool> ValidateResetPassword(ResetPasswordRequestDto dto)
        {
            if (dto == null)
                return ServiceResponse<bool>.Fail("Request data is required.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return ServiceResponse<bool>.Fail("Email is required.");

            if (string.IsNullOrWhiteSpace(dto.Code))
                return ServiceResponse<bool>.Fail("Code is required.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return ServiceResponse<bool>.Fail("New password is required.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                return ServiceResponse<bool>.Fail("Confirm password is required.");

            if (dto.NewPassword.Trim() != dto.ConfirmPassword.Trim())
                return ServiceResponse<bool>.Fail("New password and confirm password do not match.");

            return ServiceResponse<bool>.Ok(true);
        }

        private string BuildResetPasswordEmailBody(string code)
        {
            return $@"
                <h3>Password Reset</h3>
                <p>Your password reset code is:</p>
                <h2>{code}</h2>
                <p>This code will expire in 10 minutes.</p>";
        }

        private async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower(), ct);
        }

        private async Task<PasswordResetCode?> GetActivePasswordResetCodeAsync(User user, CancellationToken ct)
        {
            return await _context.PasswordResetCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<List<PasswordResetCode>> GetAllActiveCodesAsync(User user, CancellationToken ct)
        {
            return await _context.PasswordResetCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);
        }

        private void DisableActiveCodes(IEnumerable<PasswordResetCode> activeCodes)
        {
            foreach (var activeCode in activeCodes)
            {
                activeCode.IsUsed = true;
            }
        }
    }
}