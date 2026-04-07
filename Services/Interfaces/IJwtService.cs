using Bookify.Models.Entities;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;

namespace Bookify.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
