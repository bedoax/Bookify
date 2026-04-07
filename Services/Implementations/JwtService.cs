using Bookify.Data;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bookify.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public JwtService(IConfiguration configuration, AppDbContext context)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<string> GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //var userSubscriptionPlan = await GetUserSubscriptionPlanId(user.Id);
            var claims = new[]
            {
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name , user.Username),
                new Claim(ClaimTypes.Email , user.Email),
                new Claim ("id",user.Id.ToString()),
                //new Claim("SubscriptionPlanId" , userSubscriptionPlan.PlanId.ToString()),

            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<UserSubscription> GetUserSubscriptionPlanId(int userId)
        {
            return await _context.UserSubscriptions.FirstOrDefaultAsync(x => x.UserId == userId);
        }
        public String GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
