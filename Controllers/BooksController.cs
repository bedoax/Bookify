using Asp.Versioning;
using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.Entities;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Bookify.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [AllowAnonymous]
        [HttpGet("{bookId:int}")]
        public async Task<IActionResult> GetBookById(int bookId)
        {
            int? userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
            var result = await _bookService.GetBookByIdAsync(bookId, userId);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
        [AllowAnonymous]
        [HttpGet("author/{authorId:int}")]
        public async Task<IActionResult> GetBooksByAuthor(int authorId, [FromQuery] PaginationDto dto)
        {
            var result = await _bookService.GetBooksByAuthorAsync(authorId, dto);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [AllowAnonymous]
        [HttpGet("tags/{tagId:int}/books")]
        public async Task<IActionResult> GetBooksByAuthor(int tagId)
        {
            var result = await _bookService.GetBooksByTagIdAsync(tagId);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }


        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] BookSearchFilterDto filter, [FromQuery] PaginationDto dto)
        {
            var result = await _bookService.SearchBooksAsync(filter, dto);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookCreateDto dto)
        {
            var userId = User.GetUserId();

            var result = await _bookService.CreateBookAsync(userId, dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetBookById), new { bookId = result.Data.Id }, result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{bookId:int}")]
        public async Task<IActionResult> UpdateBook(int bookId, [FromBody] BookUpdateDto dto)
        {
            var userId = User.GetUserId();

            var result = await _bookService.UpdateBookAsync(userId, bookId, dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{bookId:int}/monetization")]
        public async Task<IActionResult> UpdateMonetizationStatus(int bookId, [FromQuery] bool isFree)
        {
            var result = await _bookService.UpdateMonetizationStatusAsync(bookId, isFree);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{bookId:int}")]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            var userId = User.GetUserId();

            var result = await _bookService.DeleteBookAsync(userId, bookId);
            if (!result.Success)
                return NotFound(result.Message);

            return NoContent();
        }
    }
}
