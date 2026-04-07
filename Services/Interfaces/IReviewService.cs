using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.Review;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResponse<ReviewDto>> CreateReviewAsync(int userId, ReviewCreateDto reviewDto, CancellationToken ct = default);
        Task<ServiceResponse<ReviewDto>> GetReviewByIdAsync(int reviewId, CancellationToken ct = default);
        Task<ServiceResponse<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, ReviewUpdateDto reviewDto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteReviewAsync(int userId, int reviewId, CancellationToken ct = default);
        Task<ServiceResponse<PaginatedResult<ReviewDto>>> GetReviewsByBookAsync(int bookId, PaginationDto dto, CancellationToken ct = default);
    }
}
