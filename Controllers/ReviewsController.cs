using Asp.Versioning;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.Review;
using Bookify.Models.Mapping;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpGet("book/{bookId:int}")]
        public async Task<IActionResult> GetBookReviews(int bookId, [FromQuery] PaginationDto dto)
        {
            var result = await _reviewService.GetReviewsByBookAsync(bookId,dto);
            if(!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [Authorize]
        [HttpGet("{reviewId:int}")]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            var result = await _reviewService.GetReviewByIdAsync(reviewId);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReviewBook([FromBody]ReviewCreateDto dto)
        {
            var userId = User.GetUserId();
            var result =await _reviewService.CreateReviewAsync(userId,dto);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction(nameof(GetReviewById),result.Data);
        }
        [Authorize]
        [HttpPut("{reviewId:int}")]
        public async Task<IActionResult> UpdateReview(int reviewId,[FromBody] ReviewUpdateDto dto)
        {
            var userId = User.GetUserId();
            var result = await _reviewService.UpdateReviewAsync(userId, reviewId,dto);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [Authorize]
        [HttpDelete("{reviewId:int}")]
        public async Task<IActionResult> DeleteReviewAsync(int reviewId)
        {
            var userId = User.GetUserId();
            var result = await _reviewService.DeleteReviewAsync(userId, reviewId);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
    }
}
