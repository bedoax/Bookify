namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  COLLECTIONS & FAVORITES
    // ─────────────────────────────────────────────

    public class Collection
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Name { get; set; } = null!;           // MaxLength(255) → Fluent

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CollectionBook> CollectionBooks { get; set; } = [];
    }
}
