using Bookify.Data;
using Bookify.Models.DTO.Stripe;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace Bookify.Services.Implementations
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly AppDbContext _context;
        private readonly ILogger<StripeService> _logger;
        public StripeService(AppDbContext context, IOptions<StripeSettings> stripeOptions, ILogger<StripeService>logger)
        {
            _stripeSettings = stripeOptions.Value; // سحب القيم من الـ Options
            _context = context;
            _logger = logger;

            // ضبط الـ API Key مرة تانية للتأكد (رغم إننا عملناها في Program.cs)
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }
        public async Task<StripePaymentResult> ChargeCustomerAsync(User user, string stripePriceId, CancellationToken ct = default)
        {
            // Get Price From Stripe
            var priceService = new PriceService();
            var price = await priceService.GetAsync(stripePriceId, null, null, ct);
            long amountInCents = price.UnitAmount ?? 0;
            string currency = price.Currency;

            // Create PaymentIntent
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                ReceiptEmail = user.Email,
                Confirm = true
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, null, ct);

            return new StripePaymentResult
            {
                Success = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                SubscriptionId = Guid.NewGuid().ToString(), // لاحقًا يمكن ربطه بـ Stripe Subscription إذا عملت Subscription API
                Message = paymentIntent.Status
            };
        }
        public async Task<ServiceResponse<string>> CreatePaymentIntentAsync(int userId, int planId, CancellationToken ct = default)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);

            if (plan == null)
                return ServiceResponse<string>.Fail("Plan not found");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(plan.Price * 100), // cents
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "planId", planId.ToString() },
            { "price", plan.Price.ToString() }
        }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options, null, ct);

            return ServiceResponse<string>.Ok(intent.ClientSecret);
        }

        // داخل ملف StripeService.cs
        public async Task<bool> CancelSubscription(string stripeSubscriptionId)
        {
            if (string.IsNullOrEmpty(stripeSubscriptionId)) return false;

            // حدد المسار بالكامل لـ Stripe لتجنب التضارب مع اسم الكلاس بتاعك
            var service = new Stripe.SubscriptionService();

            try
            {
                await service.CancelAsync(stripeSubscriptionId);
                return true;
            }
            catch (Stripe.StripeException)
            {
                return false;
            }
        }
        public async Task<ServiceResponse<PaymentIntent>> GetPaymentIntentFromWebhook(string json, string signature)
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, _stripeSettings.WebhookSecret);
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                return ServiceResponse<PaymentIntent>.Ok(stripeEvent.Data.Object as PaymentIntent);
            }
            return ServiceResponse<PaymentIntent>.Fail("Not a success event");
        }

    }
}
