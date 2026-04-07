namespace Bookify.Models.DTO.Author
{
    public class AuthorDto : IAuthor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public DateOnly? Birthdate { get; set; }
        public DateOnly? Deathdate { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
