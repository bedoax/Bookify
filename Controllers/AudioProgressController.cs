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
    //maybe merge this and bookProgress to one controller called progress conteroller
    public class AudioProgressController : ControllerBase
    {
        /*
         GET /progress/{bookId}
            POST /progress
            PUT /progress
         */
        private readonly IAudioProgressService _audioProgressService;
        public AudioProgressController(IAudioProgressService audioProgressService)
        {
            _audioProgressService = audioProgressService;
        }
        [HttpGet("{bookId:int}")]
        public async Task<IActionResult> GetProgressByIdBook(int bookId)
        {
            var userId = User.GetUserId();
            var result = await _audioProgressService.GetAudioProgressAsync(userId, bookId);
            if(!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProgress(int bookId,TimeSpan timeSpan)
        {
            var userId = User.GetUserId();
            var result = await _audioProgressService.CreateAudioProgressAsync(userId,bookId,timeSpan);
            if(!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction(nameof(CreateProgress), new { bookId ,userId} ,result.Data);
        }
        [HttpPut("{bookId:int}")]
        public async Task<IActionResult> UpdateProgress(int bookId, TimeSpan timeSpan)
        {
            var userId = User.GetUserId();
            var result = await _audioProgressService.UpdateAudioProgressAsync(userId, bookId, timeSpan);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }
    }
}
