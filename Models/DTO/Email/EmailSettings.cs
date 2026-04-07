namespace Bookify.Models.DTO.Email
{
    public class EmailSettings
    {
        public string DisplayName { get; set; } = default!;
        public string From { get; set; } = default!;
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string AppPassword { get; set; } = default!;
    }
}
