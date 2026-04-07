using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.Pagination;
using Bookify.Services.Implementations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bookify.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<ServiceResponse<bool>> AddToFavoritesAsync(int userId, int bookId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> RemoveFromFavoritesAsync(int userId, int bookId, CancellationToken ct = default);
        Task<ServiceResponse<PaginatedResult<BookCardDto>>> GetFavoritesAsync(int userId, PaginationDto dto, CancellationToken ct = default);
    }
}





