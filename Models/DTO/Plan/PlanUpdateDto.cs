using System.ComponentModel.DataAnnotations;

namespace Bookify.Models.DTO.Plan
{
    public class PlanUpdateDto : IPlan
    {

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }
        public int DurationDays { get; set; }

        public bool IsActive { get; set; } = true;
        public string? StripePriceId { get; set; }
        public int Rank { get; set; }
    }
}
