namespace Bookify.Models.DTO.Author
{
    public class AuthorUpdateDto : IAuthor
    {
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public DateOnly? Birthdate { get; set; }
        public DateOnly? Deathdate { get; set; }
    }
}
