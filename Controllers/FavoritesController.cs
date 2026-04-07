using Bookify.Models.DTO.Pagination;
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
    public class FavoritesController : ControllerBase
    {
        /*
         POST /bookmarks
        DELETE /bookmarks/{bookId}
        GET /bookmarks
         */
        private readonly IFavoriteService _favoriteService;
        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }
        [HttpGet]
        public async Task<IActionResult> GetFavorites([FromQuery] PaginationDto dto)
        {
            var userId = User.GetUserId();
            var result = await _favoriteService.GetFavoritesAsync(userId,dto);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> AddToFavorite([FromQuery] int bookId)
        {
            var userId = User.GetUserId();
            var result = await _favoriteService.AddToFavoritesAsync(userId, bookId);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction(nameof(GetFavorites),result.Data);
        }
        [HttpDelete("{bookId:int}")]
        public async Task<IActionResult> DeleteFavorite(int bookId)
        {
            var userId = User.GetUserId();
            var result = await _favoriteService.RemoveFromFavoritesAsync(userId, bookId);
            if (!result.Success)
                return NotFound(result.Message);
            return NoContent();
        }
    }
}
