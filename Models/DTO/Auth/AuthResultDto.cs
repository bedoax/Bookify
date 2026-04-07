namespace Bookify.Models.DTO.Auth
{
    public class AuthResultDto
    {
        // The "Key" to the kingdom (The JWT)
        public string AccessToken { get; set; } = null!;

        // Used to get a new AccessToken without logging in again
        public string RefreshToken { get; set; } = null!;

        // Tells the frontend exactly when to stop using the AccessToken
        public DateTime ExpiresAt { get; set; }

        // Useful for the UI to show "Welcome, [User]" immediately
        //public string Username { get; set; } = null!;
        //public string Email { get; set; } = null!;

        // Optional: Helps the UI decide which tabs (Admin/User) to show
        //public List<string> Roles { get; set; } = [];
    }
}