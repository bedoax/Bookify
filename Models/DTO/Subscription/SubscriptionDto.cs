namespace Bookify.Models.DTO.Subscription
{
    public class SubscriptionDto
    {
        // this is testing mamybe wrong so recap it after internet coming back
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public string PlanCode { get; set; }
        public int DurationDays { get; set; }
        public decimal? PriceAtPurchase { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string Status { get; set; } // active , canceled , expired
        public string? StripePriceId { get; set; } // for auto subscription renewal
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isTrial { get; set; }


    }
}
