namespace Bookify.Models.DTO.Stripe
{
    public class StripePaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = null!;
        public string SubscriptionId { get; set; } = null!;
        public string? Message { get; set; }
    }
}
