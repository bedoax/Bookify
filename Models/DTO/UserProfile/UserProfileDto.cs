namespace Bookify.Models.DTO.UserProfile
{
    public class UserProfileDto: IUserProfile
    {
        public string UserName { get; set; }
        public string Email { get; set; } = null!;
    }
}
