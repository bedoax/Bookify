namespace Bookify.Services.Implementations
{
    using global::Bookify.Data;
    using global::Bookify.Models.DTO.Narrator;
    using global::Bookify.Models.DTO.Narrto;
    using global::Bookify.Models.Entities;
    using global::Bookify.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;

    namespace Bookify.Services.Implementations
    {
        public class BookFileNarratorService : IBookFileNarratorService
        {
            private readonly AppDbContext _context;
            public BookFileNarratorService(AppDbContext context)
            {
                _context = context;
            }



            // Add narrator to a book file
            public async Task<ServiceResponse<bool>> AddNarratorToBookFileAsync(int bookFileId, int narratorId, CancellationToken ct = default)
            {
                var bookFile = await GetBookFileWithNarrtor(bookFileId, ct);

                if (bookFile == null)
                    return ServiceResponse<bool>.Fail("BookFile not found");

                var narrator = await GetNarratorById(bookFileId, ct);
                if (narrator == null)
                    return ServiceResponse<bool>.Fail("Narrator not found");

                var isLinked = await CheckNarratorLinkedToBookFile(bookFileId, narratorId, ct);

                if (isLinked)
                    return ServiceResponse<bool>.Fail("Narrator already linked to this BookFile");

                var link = CreateBookFileNarrator(bookFileId, narratorId);

                bookFile.BookFileNarrators.Add(link);
                await _context.SaveChangesAsync(ct);

                return ServiceResponse<bool>.Ok(true, "Narrator added to BookFile successfully");
            }

            // Remove narrator from a book file
            public async Task<ServiceResponse<bool>> RemoveNarratorFromBookFileAsync(int bookFileId, int narratorId, CancellationToken ct = default)
            {
                var link = await GetBookFileNarratorLink(bookFileId, narratorId, ct);

                if (link == null)
                    return ServiceResponse<bool>.Fail("Link not found");

                _context.BookFileNarrators.Remove(link);
                await _context.SaveChangesAsync(ct);

                return ServiceResponse<bool>.Ok(true, "Narrator removed from BookFile successfully");
            }

            // Get all narrators for a book file
            public async Task<ServiceResponse<List<NarratorDto>>> GetNarratorsByBookFileAsync(int bookFileId, CancellationToken ct = default)
            {
                var narrators = await GetNarratorsByBookFileProjection(bookFileId, ct);

                if (!narrators.Any())
                    return ServiceResponse<List<NarratorDto>>.Fail("No narrators found for this BookFile");

                return ServiceResponse<List<NarratorDto>>.Ok(narrators);
            }

            // Get all book files for a narrator
            public async Task<ServiceResponse<List<BookFileNarratorDto>>> GetBookFilesByNarratorAsync(int narratorId, CancellationToken ct = default)
            {
                // Projection مباشر بدون Include
                var result = await GetBookFilesByNarratorProjection(narratorId, ct);

                if (!result.Any())
                    return ServiceResponse<List<BookFileNarratorDto>>.Fail("No book files found for this narrator");

                return ServiceResponse<List<BookFileNarratorDto>>.Ok(result);
            }

            private async Task<BookFile?> GetBookFileWithNarrtor(int bookFileId, CancellationToken ct)
            {
                return await _context.BookFiles
                     .Include(bf => bf.BookFileNarrators)
                     .FirstOrDefaultAsync(bf => bf.Id == bookFileId, ct);
            }
            private async Task<Narrator?> GetNarratorById(int narratorId, CancellationToken ct)
            {
                return await _context.Narrators.FirstOrDefaultAsync(n => n.Id == narratorId, ct);
            }
            private async Task<bool> CheckNarratorLinkedToBookFile(int bookFileId, int narratorId, CancellationToken ct)
            {
                return await _context.BookFileNarrators.AnyAsync(x => x.BookFileId == bookFileId && x.NarratorId == narratorId, ct);
            }
            private BookFileNarrator CreateBookFileNarrator(int bookFileId, int narratorId)
            {
                return new BookFileNarrator
                {
                    BookFileId = bookFileId,
                    NarratorId = narratorId
                };
            }
            private async Task<BookFileNarrator?> GetBookFileNarratorLink(int bookFileId, int narratorId, CancellationToken ct)
            {
                return await _context.BookFileNarrators
                     .FirstOrDefaultAsync(x => x.BookFileId == bookFileId && x.NarratorId == narratorId, ct);
            }
            private async Task<List<BookFileNarratorDto>> GetBookFilesByNarratorProjection(int narratorId, CancellationToken ct)
            {
                return await _context.BookFileNarrators
                       .Where(bfn => bfn.NarratorId == narratorId)
                       .Select(bfn => new BookFileNarratorDto
                       {
                           BookFileId = bfn.BookFileId,
                           FileFormat = bfn.BookFile.FileFormat,
                           AudioDuration = bfn.BookFile.AudioDuration,
                           BookTitle = bfn.BookFile.Book.Title,
                           NarratorId = bfn.NarratorId,
                           NarratorName = bfn.Narrator.Name
                       })
                       .ToListAsync(ct);
            }
            private async Task<List<NarratorDto>> GetNarratorsByBookFileProjection(int bookFileId, CancellationToken ct)
            {
                return await _context.BookFileNarrators
                    .Where(bfn => bfn.BookFileId == bookFileId)
                    .Select(bfn => new NarratorDto
                    {
                        Id = bfn.Narrator.Id,
                        Name = bfn.Narrator.Name,
                        Bio = bfn.Narrator.Bio,
                        ImageUrl = bfn.Narrator.ImageUrl
                    })
                    .ToListAsync(ct);
            }
        }
    }
}
