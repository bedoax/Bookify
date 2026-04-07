using Bookify.Data;
using Bookify.Models.DTO.Collection;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Bookify.Services.Implementations
{
    public class CollectionService : ICollectionService
    {
        private readonly AppDbContext _context;
        public CollectionService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<CollectionDto>> CreateCollectionAsync(int userId, string collectionName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                return ServiceResponse<CollectionDto>.Fail("Collection name should be non empty.");
            }
            if (userId <= 0)
            {
                return ServiceResponse<CollectionDto>.Fail("Invalid UserId.");
            }
            var trimmedName = collectionName.Trim();

            var isDuplicateName = await CheckDuplicationOfNameCollection(userId, trimmedName, ct);

            if (isDuplicateName)
                return ServiceResponse<CollectionDto>.Fail("Collection name already exists.");

            var collection = CreateCollection(userId, trimmedName);
            await _context.Collections.AddAsync(collection);
            await _context.SaveChangesAsync(ct);
            return ServiceResponse<CollectionDto>.Ok(new CollectionDto
            {
                Id = collection.Id,
                Name = collection.Name
            }, "Collection created successfully.");
        }
        public async Task<ServiceResponse<IEnumerable<CollectionDto>>> GetCollectionsAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
            {
                return ServiceResponse<IEnumerable<CollectionDto>>.Fail("Invalid UserId.");
            }
            var collections = await GetAllCollectionByUserId(userId, ct);

            return ServiceResponse<IEnumerable<CollectionDto>>.Ok(collections, "Collections retrieved successfully.");
        }
        public async Task<ServiceResponse<CollectionDto>> GetCollectionByIdAsync(int userId, int collectionId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<CollectionDto>.Fail("Invalid user ID.");

            if (collectionId <= 0)
                return ServiceResponse<CollectionDto>.Fail("Invalid collection ID.");

            var collection = await GetCollectionDtoByCollectionId(userId, collectionId, ct);

            if (collection is null)
                return ServiceResponse<CollectionDto>.Fail("Collection not found.");

            return ServiceResponse<CollectionDto>.Ok(collection, "Collection retrieved successfully.");
        }
        public async Task<ServiceResponse<CollectionDto>> UpdateCollectionAsync(int userId, int collectionId, string newName, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<CollectionDto>.Fail("Invalid user ID.");

            if (collectionId <= 0)
                return ServiceResponse<CollectionDto>.Fail("Invalid collection ID.");

            if (string.IsNullOrWhiteSpace(newName))
                return ServiceResponse<CollectionDto>.Fail("Collection name cannot be empty.");

            var trimmedName = newName.Trim();

            var isDuplicateName = await CheckDuplicationOfNameCollectionForUpdate(userId, collectionId, trimmedName, ct);
            if (isDuplicateName)
                return ServiceResponse<CollectionDto>.Fail("Collection name already exists.");

            var collection = await GetCollectionByCollectionId(userId, collectionId, ct);
            if (collection is null)
                return ServiceResponse<CollectionDto>.Fail("Collection not found.");

            collection.Name = trimmedName;

            await _context.SaveChangesAsync(ct);

            var updatedDto = await GetCollectionDtoByCollectionId(userId, collectionId, ct);
            return ServiceResponse<CollectionDto>.Ok(updatedDto!, "Collection updated successfully.");
        }


        public async Task<ServiceResponse<bool>> DeleteCollectionAsync(int userId, int collectionId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid user ID.");
            if (collectionId <= 0)
                return ServiceResponse<bool>.Fail("Invalid collection ID.");

            var collection = await GetCollectionByCollectionId(userId, collectionId, ct);

            if (collection is null)
                return ServiceResponse<bool>.Fail("Collection not found");

            var bookFromCollection = await GetAllBooksFromCollection(collectionId, ct);

            try
            {
                _context.CollectionBooks.RemoveRange(bookFromCollection);
                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync(ct);
            }
            catch
            {
                return ServiceResponse<bool>.Fail("An error occurred while deleting the collection.");
            }

            return ServiceResponse<bool>.Ok(true, "Collection deleted successfully.");
        }

        public async Task<ServiceResponse<bool>> AddBookToCollectionAsync(int userId, int collectionId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid user ID.");

            if (collectionId <= 0)
                return ServiceResponse<bool>.Fail("Invalid collection ID.");

            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid book ID.");

            var collection = await GetCollectionByCollectionId(userId, collectionId, ct);
            if (collection is null)
                return ServiceResponse<bool>.Fail("Collection not found.");

            var isDuplicateBook = await CheckDuplicationOfBookOnCollection(collectionId, bookId, ct);
            if (isDuplicateBook)
                return ServiceResponse<bool>.Fail("Book already exists in the collection.");

            var collectionBook = CreateBookToAddToCollection(collectionId, bookId);

            await _context.CollectionBooks.AddAsync(collectionBook, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, "Book added to collection successfully.");
        }
        public async Task<ServiceResponse<bool>> RemoveBookFromCollectionAsync(int userId, int collectionId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid user ID.");
            if (collectionId <= 0)
                return ServiceResponse<bool>.Fail("Invalid collection ID.");
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid book ID.");

            var collection = await GetCollectionByCollectionId(userId, collectionId, ct);
            if (collection is null)
                return ServiceResponse<bool>.Fail("Collection not found.");

            var collectionBook = await GetBookInCollectionById(collectionId, bookId, ct);

            if (collectionBook == null)
                return ServiceResponse<bool>.Fail("Book not found in the collection.");

            _context.CollectionBooks.Remove(collectionBook);
            await _context.SaveChangesAsync(ct);
            return ServiceResponse<bool>.Ok(true, "Book removed from collection successfully.");
        }
        private CollectionBook CreateBookToAddToCollection(int collectionId, int bookId)
        {
            return new CollectionBook
            {
                CollectionId = collectionId,
                BookId = bookId,
                AddedAt = DateTime.UtcNow
            };
        }
        private Collection CreateCollection(int userId, string collectionName)
        {
            return new Collection
            {
                Name = collectionName,
                UserId = userId
            };
        }
        private async Task<IEnumerable<CollectionDto>> GetAllCollectionByUserId(int userId, CancellationToken ct)
        {
            return await _context.Collections
                .Where(c => c.UserId == userId)
                .Select(c => new CollectionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    BookCount = c.CollectionBooks.Count
                })
                .ToListAsync(ct);
        }
        private async Task<CollectionDto?> GetCollectionDtoByCollectionId(int userId, int collectionId, CancellationToken ct)
        {
            return await _context.Collections
                .AsNoTracking()
                  .Where(c => c.Id == collectionId && c.UserId == userId)
                  .Select(c => new CollectionDto
                  {
                      Id = c.Id,
                      Name = c.Name,
                      BookCount = c.CollectionBooks.Count
                  })
                  .FirstOrDefaultAsync(ct);
        }
        private async Task<Collection?> GetCollectionByCollectionId(int userId, int collectionId, CancellationToken ct)
        {
            return await _context.Collections
                .AsNoTracking()
                  .Where(c => c.Id == collectionId && c.UserId == userId)
                  .FirstOrDefaultAsync(ct);
        }
        private async Task<bool> CheckDuplicationOfNameCollection(int userId, string collectionName, CancellationToken ct)
        {
            return await _context.Collections
                .AsNoTracking()
                .AnyAsync(c => c.UserId == userId && c.Name == collectionName.Trim(), ct);
        }
        private async Task<bool> CheckDuplicationOfNameCollectionForUpdate(int userId, int collectionId, string collectionName, CancellationToken ct)
        {
            return await _context.Collections
                .AsNoTracking()
                .AnyAsync(c => c.UserId == userId && c.Id != collectionId && c.Name == collectionName, ct);
        }
        private async Task<bool> CheckDuplicationOfBookOnCollection(int collectionId, int bookId, CancellationToken ct)
        {
            return await _context.CollectionBooks.AsNoTracking().AnyAsync(c => c.CollectionId == collectionId && c.BookId == bookId, ct);
        }
        private async Task<CollectionBook?> GetBookInCollectionById(int collectionId, int bookId, CancellationToken ct)
        {
            return await _context.CollectionBooks.FirstOrDefaultAsync(cb => cb.CollectionId == collectionId && cb.BookId == bookId, ct);
        }
        private async Task<IEnumerable<CollectionBook>> GetAllBooksFromCollection(int collectionId, CancellationToken ct)
        {
            return await _context.CollectionBooks.AsNoTracking().Where(cb => cb.CollectionId == collectionId).ToListAsync(ct);
        }


    }
}