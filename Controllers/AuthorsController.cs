using Asp.Versioning;
using Bookify.Models.DTO.Author;
using Bookify.Models.DTO.Collection;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Implementations;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAuthors([FromQuery] PaginationDto dto)
        {
            var result = await _authorService.GetAuthorsAsync(dto.PageNumber, dto.PageSize);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
        [AllowAnonymous]
        [HttpGet("{authorId:int}")]
        public async Task<IActionResult> GetAuthorById(int authorId)
        {
            var result = await _authorService.GetAuthorByIdAsync(authorId);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [AllowAnonymous]
        [HttpGet("{authorName}")]
        public async Task<IActionResult> GetAuthorById(string authorName)
        {
            var result = await _authorService.GetAuthorByNameAsync(authorName);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var userId = User.GetUserId();

            var result = await _authorService.CreateAuthorAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetAuthorById),
                new { authorId = result?.Data?.Id },
                result?.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{authorId:int}")]
        public async Task<IActionResult> UpdateAuthor(int authorId, [FromBody] AuthorUpdateDto dto)
        {
            var userId = User.GetUserId();

            var result = await _authorService.UpdateAuthorAsync(userId, authorId, dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("{authorId:int}")]
        public async Task<IActionResult> DeleteAuthor(int authorId)
        {
            var userId = User.GetUserId();

            var result = await _authorService.DeleteAuthorAsync(userId, authorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return NoContent();
        }
    }
}
