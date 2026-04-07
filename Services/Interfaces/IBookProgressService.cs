using Bookify.Models.DTO.Book;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IBookProgressService
    {
        // do the calculateed progress percentage in the service layer and just pass the last page read from the client
        Task<ServiceResponse<bool>> UpdateBookProgressAsync(int userId, int bookId, int lastPage, CancellationToken ct = default);
        Task<ServiceResponse<BookProgressDto>> GetBookProgressAsync(int userId, int bookId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> CreateBookProgressAsync(int userId, int bookId, int lastPage, CancellationToken ct = default);
    }
}
