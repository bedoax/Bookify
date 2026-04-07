using Bookify.Models.Entities;

namespace Bookify.Services.Interfaces
{
    public interface IPasswordService
        {
            string HashPassword(User user, string password);
            bool VerifyPassword(User user, string hashedPassword, string providedPassword);
        }
 }


