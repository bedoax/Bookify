namespace Bookify.Models.Entities
{
    public class Narrator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ? Bio { get; set; }
        public string ? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ICollection<BookFileNarrator> BookFileNarrators { get; set; } = new List<BookFileNarrator>();

    }
}
