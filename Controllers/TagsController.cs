using Bookify.Models.DTO.Tag;
using Bookify.Models.Entities;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        var result = await _tagService.GetAllTagsAsync();

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("{tagId:int}")]
    public async Task<IActionResult> GetTagById(int tagId)
    {
        var result = await _tagService.GetTagByIdAsync(tagId);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Data);
    }
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagDto request)
    {
        var userId = User.GetUserId();

        var result = await _tagService.CreateTagAsync(userId, request.Name);

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(
                nameof(GetTagById),
                new { tagId = result.Data.Id },
                result.Data);
    }
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{tagId:int}")]
    public async Task<IActionResult> UpdateTag(int tagId, [FromBody] UpdateTagDto request)
    {
        var userId = User.GetUserId();

        var result = await _tagService.UpdateTagAsync(userId, tagId, request.Name);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }
    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{tagId:int}")]
    public async Task<IActionResult> DeleteTag(int tagId)
    {
        var userId = User.GetUserId();

        var result = await _tagService.DeleteTagAsync(userId, tagId);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }
}
