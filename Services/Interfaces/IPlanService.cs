using Bookify.Models.DTO.Plan;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IPlanService
    {
        Task<ServiceResponse<PlanDto>> CreatePlanAsync(int userId, PlanCreateDto planDto, CancellationToken ct = default);
        Task<ServiceResponse<PlanDto>> GetPlanByIdAsync(int planId, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<PlanDto>>> GetActivePlansAsync(CancellationToken ct = default);
        Task<ServiceResponse<bool>> UpdatePlanAsync(int userId, int planId, PlanUpdateDto planDto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> UpdatePlanStatusAsync(int userId,int planId, bool isActive, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<PlanDto>>> GetAllPlansAsync(CancellationToken ct = default);

    }
}