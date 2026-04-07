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
    //maybe merge this and audioProgress to one controller called progress conteroller

    public class BookProgressController : ControllerBase
    {
        private readonly IBookProgressService _bookProgressService;
        public BookProgressController(IBookProgressService bookProgressService)
        {
            _bookProgressService = bookProgressService;
        }
        [HttpGet("{bookId:int}")]
        public async Task<IActionResult> GetProgressByIdBook(int bookId)
        {
            var userId = User.GetUserId();
            var result = await _bookProgressService.GetBookProgressAsync(userId, bookId);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProgress(int bookId, int page)
        {
            var userId = User.GetUserId();
            var result = await _bookProgressService.CreateBookProgressAsync(userId, bookId, page);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction(nameof(CreateProgress), new { bookId, userId }, result.Data);
        }
        [HttpPut("{bookId:int}")]
        public async Task<IActionResult> UpdateProgress(int bookId, int page)
        {
            var userId = User.GetUserId();
            var result = await _bookProgressService.UpdateBookProgressAsync(userId, bookId, page);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }
    }
}
