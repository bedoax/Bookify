using Bookify.Data;
using Bookify.Models.DTO.Plan;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class PlanService : IPlanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PlanService> _logger;

        public PlanService(AppDbContext context, ILogger<PlanService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<PlanDto>> CreatePlanAsync(int userId, PlanCreateDto planDto, CancellationToken ct = default)
        {
            // 1. Validation (ممكن تضيف فحص لو الـ Rank مكرر لو تحب)
            if (string.IsNullOrWhiteSpace(planDto.Name))
                return ServiceResponse<PlanDto>.Fail("Plan name is required.");

            var code = GeneratePlanCode(planDto.Name);

            if (await _context.SubscriptionPlans.AnyAsync(p => p.Code == code, ct))
                return ServiceResponse<PlanDto>.Fail("A plan with this code already exists.");

            var plan = new SubscriptionPlan
            {
                Name = planDto.Name.Trim(),
                Code = code,
                Price = planDto.Price,
                Rank = planDto.Rank, // الرتبة الجديدة
                DurationDays = planDto.DurationDays,
                IsActive = planDto.IsActive,
                StripePriceId = planDto.StripePriceId,
                CreatedAt = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<PlanDto>.Ok(MapToPlanDto(plan));
        }

        public async Task<ServiceResponse<bool>> UpdatePlanAsync(int userId, int planId, PlanUpdateDto planDto, CancellationToken ct = default)
        {

            int affectedRows = await UpdatePlan(planId, planDto, ct);

            if (affectedRows == 0)
                return ServiceResponse<bool>.Fail("Plan not found.");

            return ServiceResponse<bool>.Ok(true, "Plan updated successfully.");
        }

        public async Task<ServiceResponse<IEnumerable<PlanDto>>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _context.SubscriptionPlans
                .AsNoTracking()
                .OrderBy(p => p.Rank)
                .Select(p => MapToPlanDto(p))
                .ToListAsync(ct);

            return ServiceResponse<IEnumerable<PlanDto>>.Ok(plans);
        }



        private static string GeneratePlanCode(string name) => name.Replace(" ", "_").ToLower() + "_plan";

        private async Task<PlanDto?> GetPlanDtoById(int planId, CancellationToken ct)
        {
            return await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.Id == planId)
                .Select(p => MapToPlanDto(p))
                .FirstOrDefaultAsync(ct);
        }
        private static PlanDto MapToPlanDto(SubscriptionPlan p) => new PlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Price = p.Price,
            Rank = p.Rank,
            DurationDays = p.DurationDays,
            IsActive = p.IsActive
        };

        public async Task<ServiceResponse<PlanDto>> GetPlanByIdAsync(int planId, CancellationToken ct = default)
        {
            if (planId <= 0)
                return ServiceResponse<PlanDto>.Fail("Invalid plan ID.");

            var planDto = await GetPlanDtoById(planId, ct);
            if (planDto is null)
                return ServiceResponse<PlanDto>.Fail("Plan not found.");

            return ServiceResponse<PlanDto>.Ok(planDto!, "Plan retrieved successfully.");
        }

        public async Task<ServiceResponse<IEnumerable<PlanDto>>> GetActivePlansAsync(CancellationToken ct = default)
        {

            var activePlans = await GetActivePlanDto(ct);

            return ServiceResponse<IEnumerable<PlanDto>>.Ok(activePlans, "Active plans retrieved successfully.");
        }

        public async Task<ServiceResponse<bool>> UpdatePlanStatusAsync(int userId, int planId, bool isActive, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid user ID.");
            var userExists = await CheckUserExists(userId, ct);
            if (!userExists)
                return ServiceResponse<bool>.Fail("User not found.");

            if (planId <= 0)
                return ServiceResponse<bool>.Fail("Invalid plan ID.");
            var plan = await GetPlanByid(planId, ct);
            if (plan is null)
                return ServiceResponse<bool>.Fail("Plan not found.");
            plan.IsActive = isActive;
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, $"Plan {(isActive ? "activated" : "deactivated")} successfully.");
        }
        private async Task<IEnumerable<PlanDto>> GetActivePlanDto(CancellationToken ct)
        {
            return await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Rank)
                .Select(p => MapToPlanDto(p))
                .ToListAsync(ct);
        }
        private async Task<SubscriptionPlan?> GetPlanByid(int planId, CancellationToken ct)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planId, ct);
        }
        private async Task<bool> CheckUserExists(int userId, CancellationToken ct)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId, ct);
        }
        private async Task<int> UpdatePlan(int planId, PlanUpdateDto planDto, CancellationToken ct)
        {
            return await _context.SubscriptionPlans
                 .Where(p => p.Id == planId)
                 .ExecuteUpdateAsync(setters => setters
                     .SetProperty(p => p.Name, planDto.Name.Trim())
                     .SetProperty(p => p.Price, planDto.Price)
                     .SetProperty(p => p.Rank, planDto.Rank)
                     .SetProperty(p => p.DurationDays, planDto.DurationDays)
                     .SetProperty(p => p.IsActive, planDto.IsActive),
                     ct);
        }
    }
}