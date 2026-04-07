using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.Pagination;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IBookService
    {
        Task<ServiceResponse<BookDto>> CreateBookAsync(int userId, BookCreateDto bookDto, CancellationToken ct = default);
        Task<ServiceResponse<BookDto>> GetBookByIdAsync(int bookId, int? userId = null, CancellationToken ct = default);
        Task<ServiceResponse<List<BookDto>>> GetBooksByTagIdAsync(int tagId, CancellationToken ct = default);
        Task<ServiceResponse<BookDto>> UpdateBookAsync(int userId, int bookId, BookUpdateDto bookDto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteBookAsync(int userId, int bookId, CancellationToken ct = default);

        // Enterprise monetization toggle (moved from PlanService)
        Task<ServiceResponse<bool>> UpdateMonetizationStatusAsync(int bookId, bool isFree, CancellationToken ct = default);

        // Advanced Search & Browse
        Task<ServiceResponse<PaginatedResult<BookDto>>> SearchBooksAsync(BookSearchFilterDto filter, PaginationDto dto, CancellationToken ct = default);
        Task<ServiceResponse<PaginatedResult<BookDto>>> GetBooksByAuthorAsync(int authorId, PaginationDto dto, CancellationToken ct = default);
    }
}
