using System.ComponentModel.DataAnnotations;

namespace Bookify.Models.DTO.Book
{
    public class BookCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [Required]
        public DateTime PublishDate { get; set; }
        public int Rank { get; set; }   
        public bool IsFree { get; set; }

        //Use IDs for many-to-many links
        [MinLength(1, ErrorMessage = "A book must have at least one author.")]
        public List<int> AuthorIds { get; set; } = [];
        [MinLength(1,ErrorMessage = "A tag should must have at least one tag")]
        public List<int> TagIds { get; set; } = [];

        // --- File Metadata (Mapped to BookFile entity in Service) ---
        public string? PdfUrl { get; set; }
        public string? AudioUrl { get; set; }
        public int? PageCount { get; set; }
        public decimal? FileSize { get; set; }
        public TimeSpan? AudioDuration { get; set; }
        public List<int>? NarratorIds { get; set; }

        [StringLength(50)]
        public string Language { get; set; } = "English";
    }
}