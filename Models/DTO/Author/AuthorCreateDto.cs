using System.ComponentModel.DataAnnotations;

namespace Bookify.Models.DTO.Author
{
    public class AuthorCreateDto : IAuthor
    {
        [Required(ErrorMessage = "Every story needs a creator. Please provide a name.")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!; // Auto-property, no exception!

        [StringLength(2000, ErrorMessage = "A bio should be a glimpse, not a whole book!")]
        public string? Bio { get; set; }

        public DateOnly? Birthdate { get; set; }
        public DateOnly? Deathdate { get; set; }
    }
    public interface IAuthor
    {
        // The contract: Anyone who is an "Author" must have these
        string Name { get; set; }
        string? Bio { get; set; }
        DateOnly? Birthdate { get; set; }
        DateOnly? Deathdate { get; set; }
    }
}
