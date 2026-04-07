namespace Bookify.Models.DTO.Plan
{
    public class PlanDto: IPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }
        public string? StripePriceId { get; set; }
        public int Rank { get; set; }
    }

}
