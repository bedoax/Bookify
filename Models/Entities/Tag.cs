namespace Bookify.Models.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;           // MaxLength(100) → Fluent
        public bool IsDeleted { get; set; } = false;
        public ICollection<BookTag> BookTags { get; set; } = [];
    }
}
