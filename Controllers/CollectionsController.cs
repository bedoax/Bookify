using Bookify.Models.DTO.Collection;
using Bookify.Models.Entities;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCollections()
    {
        var userIdClaim = User.GetUserId();
        if (userIdClaim == null) return Unauthorized();

        var userId = User.GetUserId();

        var result = await _collectionService.GetCollectionsAsync(userId);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Data);
    }

    [HttpGet("{collectionId:int}")]
    public async Task<IActionResult> GetCollectionById(int collectionId)
    {
        var userIdClaim = User.GetUserId();
        if (userIdClaim == null) return Unauthorized();

        var userId = User.GetUserId();

        var result = await _collectionService.GetCollectionByIdAsync(userId, collectionId);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Data);
    }
    [Authorize(Roles = AppRoles.User)]
    [HttpPost]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required");

        var userId = User.GetUserId();

        var result = await _collectionService.CreateCollectionAsync(userId, dto.Name);

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetCollectionById),
            new { collectionId = result.Data.Id },
            result.Data);
    }
    [Authorize(Roles = AppRoles.User)]
    [HttpPost("add-book")]
    public async Task<IActionResult> AddBookToCollection([FromBody] BookCollectionDto dto)
    {
        var userId = User.GetUserId();

        var result = await _collectionService.AddBookToCollectionAsync(userId, dto.CollectionId,dto.BookId);

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(nameof(GetCollectionById),
            new { collectionId = result.Data },
            result.Data);
    }
    [Authorize(Roles = AppRoles.User)]
    [HttpPut("{collectionId:int}")]
    public async Task<IActionResult> UpdateCollection(int collectionId, [FromBody] UpdateCollectionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required");

        var userId = User.GetUserId();

        var result = await _collectionService.UpdateCollectionAsync(userId, collectionId, dto.Name);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }
    [Authorize(Roles = AppRoles.User)]
    [HttpDelete("{collectionId:int}")]
    public async Task<IActionResult> DeleteCollection(int collectionId)
    {
        var userId = User.GetUserId();

        var result = await _collectionService.DeleteCollectionAsync(userId, collectionId);

        if (!result.Success)
            return BadRequest(result.Message);

        return NoContent(); 
    }

    [Authorize(Roles = AppRoles.User)]
    [HttpDelete("{collectionId:int}/books/{bookId:int}")]
    public async Task<IActionResult> DeleteBookFromCollection( int collectionId,  int bookId)
    {
        var userId = User.GetUserId();

        var result = await _collectionService.RemoveBookFromCollectionAsync(userId, collectionId, bookId);

        if (!result.Success)
            return BadRequest(result.Message);

        return NoContent();
    }
}