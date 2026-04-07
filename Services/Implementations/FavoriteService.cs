using Bookify.Data;
using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AppDbContext _context;
        public FavoriteService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<bool>> AddToFavoritesAsync(int userId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid UserId");
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid BookId");

            var isExist = await CheckIfBookInFovirte(userId, bookId, ct);
            if (isExist)
                return ServiceResponse<bool>.Fail("The Book is in the favorite already");
            var favoriteBook = CreateFavoriteBook(userId, bookId);

            await _context.FavoriteBooks.AddAsync(favoriteBook);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, "The book had added");
        }

        public async Task<ServiceResponse<PaginatedResult<BookCardDto>>> GetFavoritesAsync(int userId, PaginationDto dto, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<PaginatedResult<BookCardDto>>.Fail("Invalid UserId");

            var favorites = await GetAllFavoriteBooks(userId, dto.PageNumber, dto.PageSize, ct);

            return ServiceResponse<PaginatedResult<BookCardDto>>.Ok(favorites, "Data Had Fetched Correctly");
        }

        public async Task<ServiceResponse<bool>> RemoveFromFavoritesAsync(int userId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid UserId");
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid BookId");

            var favoriteBook = await GetFavoriteDataBook(userId, bookId);
            if (favoriteBook == null)
                return ServiceResponse<bool>.Fail("The BookID not found in the favorite");

            _context.FavoriteBooks.Remove(favoriteBook);

            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, "Book Had Removed From Favorite");
        }

        private async Task<bool> CheckIfBookInFovirte(int userId, int bookId, CancellationToken ct)
        {
            return await _context.FavoriteBooks.AnyAsync(b => b.UserId == userId && b.BookId == bookId, ct);
        }
        private FavoriteBook CreateFavoriteBook(int userId, int bookId)
        {
            return new FavoriteBook
            {
                BookId = bookId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
        }
        private async Task<FavoriteBook?> GetFavoriteDataBook(int userId, int bookId)
        {
            return await _context.FavoriteBooks.FirstOrDefaultAsync(u => u.UserId == userId && u.BookId == bookId);
        }


        private async Task<PaginatedResult<BookCardDto>> GetAllFavoriteBooks(int userId, int pageNumber, int pageSize, CancellationToken ct = default)
        {

            var query = GetBooksFromFavoriteBook(userId);


            var totalCount = await query.CountAsync(ct);

            var items = await GetBooksFavoriteCard(query, pageNumber, pageSize, ct);

            return new PaginatedResult<BookCardDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        private IQueryable<Book> GetBooksFromFavoriteBook(int userId)
        {
            return _context.FavoriteBooks
                 .Where(f => f.UserId == userId)
                 .Select(f => f.Book);
        }
        private async Task<IEnumerable<BookCardDto>> GetBooksFavoriteCard(IQueryable<Book> query, int pageNumber, int pageSize, CancellationToken ct)
        {
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookCardDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    CoverImageUrl = b.CoverImageUrl,
                    IsFree = b.IsFree,
                    AuthorNames = b.BookAuthors.Select(ba => ba.Author.Name).ToList()
                })
                .ToListAsync(ct);
        }
        /*        private async Task<PaginatedResult<BookDto>> GetAllForiteBooks(int userId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var query = await GetBooksFromFavoriteBook(userId);

            var totalCount = await query.CountAsync(ct);

            var items = await GetBooksDtoFromFavoriteBook(query, pageNumber, pageSize, ct);

            return new PaginatedResult<BookDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private async Task<IEnumerable<BookDto>> GetBooksDtoFromFavoriteBook(IQueryable<Book> query, int pageNumber, int pageSize, CancellationToken ct)
        {
            return await query
                  .Skip((pageNumber - 1) * pageSize)
                  .Take(pageSize)
                  .Select(b => new BookDto
                  {
                      Id = b.Id,
                      Title = b.Title,
                      Description = b.Description,
                      CoverImageUrl = b.CoverImageUrl,
                      IsFree = b.IsFree,
                      PublishedAt = b.PublishedAt,

                      PdfUrl = b.Files
                          .Where(f => f.FileFormat == "PDF")
                          .Select(f => f.CdnUrl)
                          .FirstOrDefault(),

                      AudioUrl = b.Files
                          .Where(f => f.FileFormat == "MP3" || f.FileFormat == "M4B")
                          .Select(f => f.CdnUrl)
                          .FirstOrDefault(),

                      PageCount = b.Files
                          .Where(f => f.FileFormat == "PDF" || f.FileFormat == "EPUB")
                          .Max(f => f.PageCount),

                      AudioDuration = b.Files
                          .Where(f => f.FileFormat == "MP3" || f.FileFormat == "M4B")
                          .Select(f => f.AudioDuration)
                          .FirstOrDefault() != null
                              ? TimeSpan.FromSeconds(b.Files
                                  .Where(f => f.FileFormat == "MP3" || f.FileFormat == "M4B")
                                  .Select(f => (double)f.AudioDuration!)
                                  .FirstOrDefault())
                              : null,

                      AuthorIds = b.BookAuthors.Select(ba => ba.AuthorId).ToList(),
                      AuthorNames = b.BookAuthors.Select(ba => ba.Author.Name).ToList(),
                      TagIds = b.BookTags.Select(bt => bt.TagId).ToList(),
                      TagNames = b.BookTags.Select(bt => bt.Tag.Name).ToList()
                  })
                  .ToListAsync(ct);
        }*/
    }
}





