using Bookify.Models.Entities;

namespace Bookify.Models.DTO.Narrator
{
    public class NarratorUpdateDto : INarrator
    {
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
    }
}
