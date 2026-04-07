using Bookify.Models.DTO.Subscription;
using Bookify.Models.Entities;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<ServiceResponse<SubscriptionDto>> SubscribeAsync(int userId, int planId, CancellationToken ct = default);
        Task<ServiceResponse<SubscriptionDto>> GetUserSubscriptionAsync(int userId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> UnsubscribeAsync(int userId, CancellationToken ct = default);

        // Critical enterprise method: Verifies access before streaming/downloading
        Task<ServiceResponse<bool>> HasAccessToBookAsync(int userId, int bookId, CancellationToken ct = default);
        Task<UserSubscription> CreateFreeSubscription(int  userId);
        Task<ServiceResponse<bool>> StartFreeTrialAsync(int userId, CancellationToken ct = default);
    }
}




        


    