using Bookify.Models.DTO.Collection;
using Bookify.Services.Implementations;


namespace Bookify.Services.Interfaces
{
    public interface ICollectionService
    {
        Task<ServiceResponse<CollectionDto>> CreateCollectionAsync(int userId, string collectionName, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<CollectionDto>>> GetCollectionsAsync(int userId, CancellationToken ct = default);
        Task<ServiceResponse<CollectionDto>> GetCollectionByIdAsync(int userId, int collectionId, CancellationToken ct = default);
        Task<ServiceResponse<CollectionDto>> UpdateCollectionAsync(int userId, int collectionId, string newName, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteCollectionAsync(int userId, int collectionId, CancellationToken ct = default);

        // Link table operations
        Task<ServiceResponse<bool>> AddBookToCollectionAsync(int userId, int collectionId, int bookId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> RemoveBookFromCollectionAsync(int userId, int collectionId, int bookId, CancellationToken ct = default);
    }
}