using Bookify.Data;
using Bookify.Models.DTO.Book;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class BookProgressService : IBookProgressService
    {
        private readonly AppDbContext _context;
        public BookProgressService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<BookProgressDto>> GetBookProgressAsync(int userId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<BookProgressDto>.Fail("Invalid User id");
            if (bookId <= 0)
                return ServiceResponse<BookProgressDto>.Fail("Invalid Book id");


            var bookProgress = await FetchBookInformation(userId, bookId, ct);

            if (bookProgress is null)
                return ServiceResponse<BookProgressDto>.Fail("BookProgress not found");
            return ServiceResponse<BookProgressDto>.Ok(bookProgress);
        }
        public async Task<ServiceResponse<bool>> CreateBookProgressAsync(int userId, int bookId, int lastPage, CancellationToken ct)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid User id");
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Book id");
            if (lastPage <= 0)
                return ServiceResponse<bool>.Fail("Invalid lastPage");

            var newProgress = new UserBookProgress
            {
                UserId = userId,
                BookId = bookId,
                LastReadPage = lastPage,
                UpdatedAt = DateTime.UtcNow,

            };

            await _context.UserBookProgress.AddAsync(newProgress, ct);
            await _context.SaveChangesAsync(ct);
            return ServiceResponse<bool>.Ok(true, "Book Progress Has Created");
        }
        public async Task<ServiceResponse<bool>> UpdateBookProgressAsync(int userId, int bookId, int lastPage, CancellationToken ct = default)
        {
            /*
             there other solution for update to track it to make a busness logic like if user want to save the lower pages instade of bigger , or just bigger only , or apply things like that 
              so u will use the track , to get entity and apply then update instad of execute update
             */
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid User id");
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Book id");
            try
            {

                var affectedRows = await _context.UserBookProgress
                    .Where(u => u.UserId == userId && u.BookId == bookId && u.LastReadPage < lastPage)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.LastReadPage, lastPage)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow),
                        ct);


                if (affectedRows == 0)
                {
                    var exists = await isUserBookProgressExist(userId, bookId, ct);

                    if (!exists)
                        await CreateBookProgressAsync(userId, bookId, lastPage, ct);
                }

                return ServiceResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {

                return ServiceResponse<bool>.Fail("Sorry, there error  in updating the book progress");
            }


        }

        private async Task<BookProgressDto?> FetchBookInformation(int userId, int bookId, CancellationToken ct)
        {
            return await _context.Books
                        // if not worked then remove the comment and try again
                        /*                    .Include(b=>b.Files)
                                            .Include(b=>b.ReadersProgress)*/
                        .Where(b => b.Id == bookId)
                        .Select(b => new
                        {

                            TotalPages = b.Files.Select(f => (int?)f.PageCount).FirstOrDefault() ?? 0,
                            UserProgress = b.ReadersProgress.FirstOrDefault(p => p.UserId == userId)
                        })
                        .Select(temp => new BookProgressDto
                        {
                            TotalPages = temp.TotalPages,
                            CurrentPage = temp.UserProgress != null ? (int)temp.UserProgress.LastReadPage : 0,
                            LastAccessedAt = temp.UserProgress != null ? (DateTime)temp.UserProgress.UpdatedAt : DateTime.UtcNow,

                            // حساب النسبة المئوية بدقة مع تجنب القسمة على صفر
                            ProgressPercentage = (temp.TotalPages > 0 && temp.UserProgress != null)
                                ? Math.Round(((double)temp.UserProgress.LastReadPage / temp.TotalPages) * 100, 2)
                                : 0
                        })
                        .FirstOrDefaultAsync(ct);
        }
        private async Task<UserBookProgress?> FetchUserBookProgressAsync(int userId, int bookId, CancellationToken ct)
        {
            return await _context.UserBookProgress.FirstOrDefaultAsync(u => u.UserId == userId && u.BookId == bookId, ct);
        }
        private async Task<bool> isUserBookProgressExist(int userId, int bookId, CancellationToken ct)
        {
            return await _context.UserBookProgress
                .AnyAsync(x => x.UserId == userId && x.BookId == bookId, ct);
        }
    }
}
