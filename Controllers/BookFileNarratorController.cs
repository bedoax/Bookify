using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bookify.Models.DTO.Narrator;
namespace Bookify.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookFileNarratorController : ControllerBase
    {
        private readonly IBookFileNarratorService _service;

        public BookFileNarratorController(IBookFileNarratorService service)
        {
            _service = service;
        }

        // Add a narrator to a book file
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost("AddNarrator")]
        public async Task<IActionResult> AddNarrator([FromQuery] int bookFileId, [FromQuery] int narratorId)
        {
            var result = await _service.AddNarratorToBookFileAsync(bookFileId, narratorId);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        // Remove a narrator from a book file
        [Authorize(Roles = AppRoles.Admin)]
        [HttpDelete("RemoveNarrator")]
        public async Task<IActionResult> RemoveNarrator([FromQuery] int bookFileId, [FromQuery] int narratorId)
        {
            var result = await _service.RemoveNarratorFromBookFileAsync(bookFileId, narratorId);
            if (!result.Success)
                return BadRequest(result.Message);

            return NoContent();
        }

        // Get all narrators for a book file
        [AllowAnonymous]
        [HttpGet("bookfiles/{bookFileId:int}/narrators")]
        public async Task<IActionResult> GetNarratorsByBookFile(int bookFileId)
        {
            var result = await _service.GetNarratorsByBookFileAsync(bookFileId);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        // Get all book files for a narrator
        [AllowAnonymous]
        [HttpGet("narrators/{narratorId:int}/bookfiles")]
        public async Task<IActionResult> GetBookFilesByNarrator(int narratorId)
        {
            var result = await _service.GetBookFilesByNarratorAsync(narratorId);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }
    }
}
