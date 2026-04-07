namespace Bookify.Models.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>
        /// SHA-256 hex of the raw token — fixed 64 chars.
        /// Never store the raw token.
        /// </summary>
        public string TokenHash { get; set; } = null!;      // MaxLength(64) → Fluent

        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
