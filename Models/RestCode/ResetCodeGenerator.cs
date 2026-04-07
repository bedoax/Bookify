using System.Security.Cryptography;
using System.Text;
namespace Bookify.Models.RestCode
{
    public static class ResetCodeGenerator
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateCode(int length = 4)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new StringBuilder(length);

            foreach (var b in bytes)
            {
                result.Append(Chars[b % Chars.Length]);
            }

            return result.ToString();
        }
    }
}
