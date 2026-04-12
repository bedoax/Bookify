using Asp.Versioning;
using Bookify.Models.DTO.Narrator;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Bookify.Controllers
{

        [ApiController]
         [Route("api/v{version:apiVersion}/[controller]")]
         [ApiVersion("1.0")]
    public class NarratorsController : ControllerBase
        {
            private readonly INarratorService _narratorService;

            public NarratorsController(INarratorService narratorService)
            {
                _narratorService = narratorService;
            }

            [AllowAnonymous]
            [HttpGet]
            public async Task<IActionResult> GetNarrators([FromQuery] PaginationDto dto)
            {
                var result = await _narratorService.GetNarratorsAsync(dto.PageNumber, dto.PageSize);
                if (!result.Success)
                    return NotFound(result.Message);

                return Ok(result.Data);
            }

            [AllowAnonymous]
            [HttpGet("{narratorId:int}")]
            public async Task<IActionResult> GetNarratorById(int narratorId)
            {
                var result = await _narratorService.GetNarratorByIdAsync(narratorId);
                if (!result.Success)
                    return NotFound(result.Message);

                return Ok(result.Data);
            }

            [Authorize(Roles = AppRoles.Admin)]
            [HttpPost]
            public async Task<IActionResult> CreateNarrator([FromBody] NarratorCreateDto dto)
            {
                if (dto == null)
                    return BadRequest("Invalid data");

                var result = await _narratorService.CreateNarratorAsync(dto);
                if (!result.Success)
                    return BadRequest(result.Message);

                return CreatedAtAction(nameof(GetNarratorById),
                    new { narratorId = result?.Data?.Id },
                    result?.Data);
            }

            [Authorize(Roles = AppRoles.Admin)]
            [HttpPut("{narratorId:int}")]
            public async Task<IActionResult> UpdateNarrator(int narratorId, [FromBody] NarratorUpdateDto dto)
            {
                var result = await _narratorService.UpdateNarratorAsync(narratorId, dto);
                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result.Data);
            }

            [Authorize(Roles = AppRoles.Admin)]
            [HttpDelete("{narratorId:int}")]
            public async Task<IActionResult> DeleteNarrator(int narratorId)
            {
                var result = await _narratorService.DeleteNarratorAsync(narratorId);
                if (!result.Success)
                    return BadRequest(result.Message);

                return NoContent();
            }
        }
}


