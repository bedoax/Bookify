namespace Bookify.Models.DTO.Plan
{
    public interface IPlan
        {
            string Name { get; set; }
            decimal Price { get; set; }
            int DurationDays { get; set; }
            bool IsActive { get; set; }
            string? StripePriceId { get; set; }
            int Rank { get; set; }
    }
}

