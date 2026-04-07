using Bookify.Models.DTO.Author;
using Bookify.Services.Implementations;


namespace Bookify.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<ServiceResponse<AuthorDto>> CreateAuthorAsync(int userId, AuthorCreateDto authorDto, CancellationToken ct = default);
        Task<ServiceResponse<AuthorDto>> GetAuthorByIdAsync(int authorId, CancellationToken ct = default);
        Task<ServiceResponse<AuthorDto>> GetAuthorByNameAsync(string authorName, CancellationToken ct = default);
        Task<ServiceResponse<AuthorDto>> UpdateAuthorAsync(int userId, int authorId, AuthorUpdateDto authorDto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteAuthorAsync(int userId, int authorId, CancellationToken ct = default);
        Task<ServiceResponse<PaginatedResult<AuthorDto>>> GetAuthorsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    }
}
