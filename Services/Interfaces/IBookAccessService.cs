using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IBookAccessService
    {
        Task<ServiceResponse<bool>> MakeBooksFreeAsync(int userId, List<int> bookIds, CancellationToken ct = default);
        Task<ServiceResponse<bool>> RemoveBooksFromFreeAsync(int userId, List<int> bookIds, CancellationToken ct = default);
        Task<ServiceResponse<bool>> SetBooksRankAsync(int userId, List<int> bookIds, int newRank, CancellationToken ct = default);
    }
}