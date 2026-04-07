using Bookify.Models.DTO.Payment;
using Bookify.Models.Mapping;
using Bookify.Services.Implementations;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
namespace Bookify.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly ISubscriptionService _subscriptionService;

        public PaymentsController(IStripeService stripeService,ILogger<PaymentsController> logger, ISubscriptionService subscriptionService)
        {
            _stripeService = stripeService;
            _logger = logger;
            _subscriptionService = subscriptionService;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto dto)
        {
            var userId = User.GetUserId();

            if (dto.PlanId <= 0)
                return BadRequest("Invalid plan id");

            var result = await _stripeService.CreatePaymentIntentAsync(userId, dto.PlanId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { clientSecret = result.Data });
        }
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var sig = Request.Headers["Stripe-Signature"];

            // 1. فك التشفير وهات الـ Intent من الـ StripeService
            var stripeResult = await _stripeService.GetPaymentIntentFromWebhook(json, sig);

            if (stripeResult.Success)
            {
                var intent = stripeResult.Data;
                // 2. ابعت الـ Metadata لخدمة الاشتراكات من هنا (الكنترولر كسر الدائرة)
                if (intent.Metadata.TryGetValue("userId", out var uId) && int.TryParse(uId, out var id))
                {
                    await _subscriptionService.SubscribeAsync(id, int.Parse(intent.Metadata["planId"]));
                }
            }
            return Ok();
        }

    }
}
