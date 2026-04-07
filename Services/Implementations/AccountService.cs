using Bookify.Data;
using Bookify.Models.DTO.Registration;
using Bookify.Models.DTO.UserProfile;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bookify.Models.Roles;

namespace Bookify.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            AppDbContext context,
            IPasswordService passwordService,
            ISubscriptionService subscriptionService,
            ILogger<AccountService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public async Task<ServiceResponse<RegistrationResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            var validationResult = ValidateRegistration(dto);
            if (!validationResult.Success)
                return ServiceResponse<RegistrationResultDto>.Fail(validationResult.Message);

            var exists = await RegistrationExistsAsync(dto, ct);
            if (exists)
                return ServiceResponse<RegistrationResultDto>.Fail("Username or email already exists.");
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var user = CreateNewUser(dto);

                user.Role = AppRoles.User;
                user.CreatedAt = DateTime.UtcNow;
                user.PasswordHash = _passwordService.HashPassword(user, dto.Password);

                await _context.Users.AddAsync(user, ct);

                await _context.SaveChangesAsync(ct);

                var freeSubscription = await _subscriptionService.CreateFreeSubscription(user.Id);

                await _context.UserSubscriptions.AddAsync(freeSubscription, ct);

                await _context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);

                return ServiceResponse<RegistrationResultDto>.Ok(new RegistrationResultDto
                {
                    Username = user.Username,
                    Email = user.Email
                }, "Registration successful.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error registering user '{Username}'", dto.Username);
                return ServiceResponse<RegistrationResultDto>.Fail("An error occurred during registration.");
            }

        }

        public async Task<ServiceResponse<UserProfileDto>> GetProfileAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<UserProfileDto>.Fail("Invalid user ID.");

            var profile = await GetUserProfileDtoAsync(userId, ct);

            if (profile is null)
                return ServiceResponse<UserProfileDto>.Fail("User not found.");

            return ServiceResponse<UserProfileDto>.Ok(profile);
        }

        public async Task<ServiceResponse<UserProfileDto>> UpdateProfileAsync(int userId, UserProfileUpdateDto dto, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<UserProfileDto>.Fail("Invalid user ID.");

            var validationResult = ValidateUserProfileUpdate(dto);
            if (!validationResult.Success)
                return ServiceResponse<UserProfileDto>.Fail(validationResult.Message);

            var user = await GetUserEntityByIdAsync(userId, ct);
            if (user is null)
                return ServiceResponse<UserProfileDto>.Fail("User not found.");

            var duplicateExists = await ProfileDataExistsForAnotherUserAsync(userId, dto, ct);
            if (duplicateExists)
                return ServiceResponse<UserProfileDto>.Fail("Username or email already exists.");

            UpdateUserProfile(user, dto);

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return ServiceResponse<UserProfileDto>.Ok(new UserProfileDto
            {
                UserName = user.Username,
                Email = user.Email
            }, "Profile updated successfully.");
        }

        private static ServiceResponse<bool> ValidateRegistration(RegisterDto dto)
        {
            if (dto is null)
                return ServiceResponse<bool>.Fail("Registration data is required.");

            if (string.IsNullOrWhiteSpace(dto.Username))
                return ServiceResponse<bool>.Fail("Username is required.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return ServiceResponse<bool>.Fail("Email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return ServiceResponse<bool>.Fail("Password is required.");

            return ServiceResponse<bool>.Ok(true, "Validation passed.");
        }

        private static ServiceResponse<bool> ValidateUserProfileUpdate(UserProfileUpdateDto dto)
        {
            if (dto is null)
                return ServiceResponse<bool>.Fail("Profile data is required.");

            if (string.IsNullOrWhiteSpace(dto.UserName))
                return ServiceResponse<bool>.Fail("Username is required.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return ServiceResponse<bool>.Fail("Email is required.");

            return ServiceResponse<bool>.Ok(true, "Validation passed.");
        }

        private async Task<bool> RegistrationExistsAsync(RegisterDto dto, CancellationToken ct = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email, ct);
        }

        private async Task<bool> ProfileDataExistsForAnotherUserAsync(int userId, UserProfileUpdateDto dto, CancellationToken ct = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Id != userId && (u.Username == dto.UserName || u.Email == dto.Email), ct);
        }

        private async Task<UserProfileDto?> GetUserProfileDtoAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new UserProfileDto
                {
                    UserName = x.Username,
                    Email = x.Email
                })
                .FirstOrDefaultAsync(ct);
        }

        private async Task<User?> GetUserEntityByIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, ct);
        }

        private static User CreateNewUser(RegisterDto dto)
        {
            return new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = dto.Password,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void UpdateUserProfile(User user, UserProfileUpdateDto dto)
        {
            user.Username = dto.UserName.Trim();
            user.Email = dto.Email.Trim();

        }
    }
}