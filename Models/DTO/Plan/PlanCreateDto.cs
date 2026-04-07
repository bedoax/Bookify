using System.ComponentModel.DataAnnotations;

namespace Bookify.Models.DTO.Plan
{
    public class PlanCreateDto:IPlan
    {

        [Required(ErrorMessage = "Give this plan a name that inspires users.")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Range(0, 10000, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Range(1, 3650, ErrorMessage = "Duration must be at least 1 day.")]
        public int DurationDays { get; set; }

        // like a season of monthes
        public bool IsActive { get; set; } = true;
        // for relate the strip with it 
        public string? StripePriceId { get; set; }
        public int Rank { get; set; }

    }
}

