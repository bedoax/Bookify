namespace Bookify.Models.Entities
{
    public class CollectionBook
    {
        public int CollectionId { get; set; }
        public Collection Collection { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public DateTime AddedAt { get; set; }
    }
}
