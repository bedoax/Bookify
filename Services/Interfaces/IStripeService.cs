using Bookify.Models.DTO.Stripe;
using Bookify.Models.Entities;
using Bookify.Services.Implementations;
using Stripe;

namespace Bookify.Services.Interfaces
{
    public interface IStripeService
    {
        Task<StripePaymentResult> ChargeCustomerAsync(User user, string SubscriptionId, CancellationToken ct);
        Task<ServiceResponse<string>> CreatePaymentIntentAsync(int userId, int planId, CancellationToken ct = default);
        Task<bool> CancelSubscription(string stripeSubscriptionId);
        Task<ServiceResponse<PaymentIntent>> GetPaymentIntentFromWebhook(string json, string signature);
    }
}




        


    