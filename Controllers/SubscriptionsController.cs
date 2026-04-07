using Bookify.Models.Mapping;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        [HttpGet("my-subscription")]
        public async Task<IActionResult> GetMySubscription()
        {
            var userId = User.GetUserId();
            var result = await _subscriptionService.GetUserSubscriptionAsync(userId);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [HttpGet("has-access/{bookId:int}")]
        public async Task<IActionResult> HasAccessBook(int bookId)
        {
            var userId = User.GetUserId();
            var result = await _subscriptionService.HasAccessToBookAsync(userId, bookId);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result.Data);
        }
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromQuery] int planId)
        {
            var userId = User.GetUserId();
            var result = await _subscriptionService.SubscribeAsync(userId, planId);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction(nameof(GetMySubscription), result.Data);
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe()
        {
            var userId = User.GetUserId();
            var result = await _subscriptionService.UnsubscribeAsync(userId);
            if (!result.Success)
                return NotFound(result.Message);
            return NoContent();
        }
        [HttpPost("free-trial")]
        public async Task<IActionResult> StartFreeTrial()
        {
            var userId = User.GetUserId();
            var result = await _subscriptionService.StartFreeTrialAsync(userId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { success = true, data = result.Data });
        }
    }
}
