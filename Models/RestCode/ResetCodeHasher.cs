using System.Security.Cryptography;
using System.Text;
namespace Bookify.Models.RestCode
{
    public static class ResetCodeHasher
    {
        public static string HashCode(string code)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(bytes);
        }
    }
}
