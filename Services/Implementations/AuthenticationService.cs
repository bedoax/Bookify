using Bookify.Data;
using Bookify.Models.DTO.Auth;
using Bookify.Models.DTO.JWT; // تأكد أن هذا الـ Namespace يحتوي على كلاس JwtSettings
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Bookify.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtOptions; // استخدام الـ Options مباشرة
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            AppDbContext context,
            IPasswordService passwordService,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtOptions,
            ILogger<AuthenticationService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value; // سحب القيم المغلفة
            _logger = logger;
        }

        public async Task<ServiceResponse<AuthResultDto>> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(loginDto.username) || string.IsNullOrEmpty(loginDto.password))
                return ServiceResponse<AuthResultDto>.Fail("Username or password is empty");

            var user = await GetUserByUsername(loginDto.username, ct);
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for username '{Username}'", loginDto.username);
                return ServiceResponse<AuthResultDto>.Fail("Invalid credentials");
            }

            var isValid = _passwordService.VerifyPassword(user, user.PasswordHash, loginDto.password);
            if (!isValid)
                return ServiceResponse<AuthResultDto>.Fail("Invalid credentials");

            var token = await _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var dbRefreshToken = CreateRefreshTokenInstance(refreshToken, user.Id);

            await _context.RefreshTokens.AddAsync(dbRefreshToken, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<AuthResultDto>.Ok(new AuthResultDto
            {
                AccessToken = token,
                // نستخدم القيمة من الـ Options بدل الـ Parse اليدوي
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.DurationInMinutes),
                RefreshToken = refreshToken,
            });
        }

        public async Task<ServiceResponse<bool>> LogoutAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid userId");

            // تحديث سريع ومباشر في الداتابيز لكل الـ Tokens النشطة لهذا المستخدم
            await _context.RefreshTokens
                 .Where(t => t.UserId == userId && !t.Revoked)
                 .ExecuteUpdateAsync(s => s
                     .SetProperty(t => t.Revoked, true)
                     .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                     .SetProperty(t => t.ExpiresAt, DateTime.UtcNow), ct);

            _logger.LogInformation("User {UserId} logged out and tokens revoked.", userId);
            return ServiceResponse<bool>.Ok(true, "Logged out successfully");
        }

        public async Task<ServiceResponse<AuthResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return ServiceResponse<AuthResultDto>.Fail("Invalid refresh token");

            var hashedToken = HashToken(refreshToken);
            var storedToken = await GetRefreshTokenDb(hashedToken, ct);

            if (storedToken == null || storedToken.Revoked || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token attempt failed: Token invalid, revoked, or expired.");
                return ServiceResponse<AuthResultDto>.Fail("Invalid or expired refresh token");
            }

            // إبطال التوكن القديم (Rotation)
            storedToken.Revoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            // إنشاء توكنات جديدة
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newDbToken = CreateRefreshTokenInstance(newRefreshToken, storedToken.UserId);
            await _context.RefreshTokens.AddAsync(newDbToken, ct);

            var newAccessToken = await _jwtService.GenerateToken(storedToken.User);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<AuthResultDto>.Ok(new AuthResultDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.DurationInMinutes)
            }, "Token refreshed successfully");
        }

        // --- Private Helpers ---

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        private RefreshToken CreateRefreshTokenInstance(string newRefreshToken, int userId)
        {
            return new RefreshToken
            {
                TokenHash = HashToken(newRefreshToken),
                UserId = userId,
                // مدة صلاحية الـ Refresh Token غالباً تكون أطول (مثلاً 7 أيام)
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task<RefreshToken?> GetRefreshTokenDb(string hashedToken, CancellationToken ct)
        {
            return await _context.RefreshTokens
                  .Include(rt => rt.User)
                  .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, ct);
        }

        private async Task<User?> GetUserByUsername(string username, CancellationToken ct)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username.Trim(), ct);
        }
    }
}