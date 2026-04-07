using Bookify.Models.DTO.Plan;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _IplanService;
        public PlansController(IPlanService IplanService)
        {
            _IplanService = IplanService;
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpGet("all-plans")]
        public async Task<IActionResult> GetAllPlans()
        {
            var result = await _IplanService.GetAllPlansAsync();
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActivePlans()
        {
            var result = await _IplanService.GetActivePlansAsync();
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [HttpGet("{planId:int}")]
        public async Task<IActionResult> GetPlanById(int planId)
        {
            var result = await _IplanService.GetPlanByIdAsync(planId);

            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody]PlanCreateDto dto)
        {
            var userId = User.GetUserId();
            var result = await _IplanService.CreatePlanAsync(userId,dto);

            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{planId:int}")]
        public async Task<IActionResult> UpdatePlan(int planId,[FromBody]PlanUpdateDto dto)
        {
            var userId = User.GetUserId();
            var result = await _IplanService.UpdatePlanAsync(userId, planId, dto);

            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("{planId:int}/status")] 
        public async Task<IActionResult> UpdatePlanStatus(int planId, [FromQuery] bool isActive)
        {
            var userId = User.GetUserId();
            var result = await _IplanService.UpdatePlanStatusAsync(userId,planId, isActive);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { PlanId = planId, IsActive = isActive, Message = result.Message });
        }
        /*        [Authorize(Roles = AppRoles.Admin)]
                [HttpPut("Reactivate/{plandId:int}")]*/

        /*        [Authorize(Roles = AppRoles.Admin)]
                [HttpDelete("{planId:int}")]
                public async Task<IActionResult> DeletePlanById(int planId)
                {
                    var userId = User.GetUserId();
                    var result = await _IplanService.DeletePlanAsync(userId, planId);

                    if (!result.Success)
                        return NotFound(result.Message);
                    return NoContent();
                }*/
    }
}
