using Bookify.Data;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class BookAccessService : IBookAccessService
    {
        private readonly AppDbContext _context;

        public BookAccessService(AppDbContext context)
        {
            _context = context;
        }

        // يجعل الكتاب مجاني للجميع (Rank = 0)
        public async Task<ServiceResponse<bool>> MakeBooksFreeAsync(int userId, List<int> bookIds, CancellationToken ct = default)
        {
            return await UpdateBooksAccessLevelInternalAsync(userId, bookIds, isFree: true, requiredRank: 0, ct);
        }

        // يجعل الكتاب مدفوع (مثلاً يرجعه لرتبة البريميوم الافتراضية 20)
        public async Task<ServiceResponse<bool>> RemoveBooksFromFreeAsync(int userId, List<int> bookIds, CancellationToken ct = default)
        {
            // هنا ممكن تخلي الرتبة الافتراضية 20 أو تمررها كباراميتر
            return await UpdateBooksAccessLevelInternalAsync(userId, bookIds, isFree: false, requiredRank: 20, ct);
        }

        // ميثود جديدة قوية جداً للأدمن للتحكم في رتبة مجموعة كتب مرة واحدة
        public async Task<ServiceResponse<bool>> SetBooksRankAsync(int userId, List<int> bookIds, int newRank, CancellationToken ct = default)
        {
            return await UpdateBooksAccessLevelInternalAsync(userId, bookIds, isFree: newRank <= 0, requiredRank: newRank, ct);
        }

        private async Task<ServiceResponse<bool>> UpdateBooksAccessLevelInternalAsync(int userId, List<int> bookIds, bool isFree, int requiredRank, CancellationToken ct)
        {
            // 1. Validation
            if (userId <= 0 || bookIds == null || !bookIds.Any())
                return ServiceResponse<bool>.Fail("Invalid data.");

            var normalizedIds = bookIds.Distinct().ToList();

            // 2. ExecuteUpdate (قوة EF Core 7+)
            // مش محتاجين Transaction هنا لأنها سطر واحد بـ Query واحدة في الداتا بيز
            var affectedRows = await _context.Books
                .Where(b => normalizedIds.Contains(b.Id) && b.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.IsFree, isFree)
                    .SetProperty(b => b.RequiredRank, requiredRank)
                    .SetProperty(b => b.UpdatedAt, DateTime.UtcNow), ct);

            if (affectedRows == 0)
                return ServiceResponse<bool>.Fail("book not found.");

            return ServiceResponse<bool>.Ok(true, "book has updated.");
        }
    }
}