using Bookify.Data;
using Bookify.Models.Constants;
using Bookify.Models.DTO.Book;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.Tag;
using Bookify.Models.Entities;
using Bookify.Services.Implementations;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    // TODO: Link books to subscription plans (PlanBooks) for monetization control.
    // TODO: Change UpdateBookAsync to be partial (PATCH semantics) rather than full replace.
    public class BookService : IBookService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookService> _logger;

        public BookService(AppDbContext context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<ServiceResponse<BookDto>> CreateBookAsync(int userId, BookCreateDto bookDto, CancellationToken ct = default)
        {
            var validationResult = ValidateBookCreate(bookDto);
            if (!validationResult.Success) return ServiceResponse<BookDto>.Fail(validationResult.Message);

            var normalizedTitle = bookDto.Title.Trim();

            if (await CheckDuplicateBookTitleOnCreateAsync(normalizedTitle, ct))
                return ServiceResponse<BookDto>.Fail("Title already exists.");

            if (bookDto.NarratorIds != null && bookDto.NarratorIds.Any())
            {
               
                var existingNarratorsCount = await GetCountOfNarratorIds(bookDto,ct);

              
                if (existingNarratorsCount != bookDto.NarratorIds.Distinct().Count())
                {
                    return ServiceResponse<BookDto>.Fail("One or more Narrator IDs are invalid.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var book = CreateBookEntity(bookDto, normalizedTitle);

                // These methods populate the navigation properties on the book entity
                // before it is added to the context, so EF Core tracks them together.
                AddBookFile(book, bookDto);
                AddBookTags(book, bookDto);
                AddBookAuthors(book, bookDto);

                await _context.Books.AddAsync(book, ct);
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return ServiceResponse<BookDto>.Ok(MapToDto(book), "Book created successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error creating book with title '{Title}'", bookDto.Title);
                var innerError = ex.InnerException?.Message ?? ex.Message;
                return ServiceResponse<BookDto>.Fail($"Database error: {innerError}");
            }
        }

        public async Task<ServiceResponse<BookDto>> GetBookByIdAsync(int bookId, int? userId, CancellationToken ct = default)
        {

            var book = await GetBookWithDetailsByIdAsync(bookId, ct);
            if (book == null) return ServiceResponse<BookDto>.Fail("Book not found.");

            var dto = MapToDto(book);

            if (userId == null || userId <= 0)
            {
                dto.CanAccess = book.IsFree && book.RequiredRank <= 0;
            }
            else
            {
                var userMaxRank = await GetUserEffectiveRankAsync(userId, ct);
                dto.CanAccess = book.IsFree || userMaxRank >= book.RequiredRank;
            }


            if (!dto.CanAccess)
            {
                dto.AudioUrl = null;
                dto.PdfUrl = null;
                dto.AudioDuration = null;
                dto.PageCount = null;
            }

            return ServiceResponse<BookDto>.Ok(dto);
        }
        public async Task<ServiceResponse<List<BookDto>>> GetBooksByTagIdAsync(int tagId, CancellationToken ct = default)
        {
            if (tagId <= 0)
                return ServiceResponse<List<BookDto>>.Fail("Invalid Tag Id");

            var tag = await GetTagById(tagId, ct);
            if (tag == null)
                return ServiceResponse<List<BookDto>>.Fail("tag is not found");

            var books = await FetchBooksByTagAsync(tagId, ct);
            return ServiceResponse<List<BookDto>>.Ok(books, $"books  by tag {tag.Name} had Found");

        }

        public async Task<ServiceResponse<BookDto>> UpdateBookAsync(int userId, int bookId, BookUpdateDto bookDto, CancellationToken ct = default)
        {
            // 1. Validation مرن (لا يشترط وجود Title)
            if (bookId <= 0 || bookDto is null)
                return ServiceResponse<BookDto>.Fail("Invalid data.");

            var book = await GetBookForUpdateAsync(bookId, ct);
            if (book is null) return ServiceResponse<BookDto>.Fail("Book not found.");

            // 2. هندلة الـ Title بحذر
            string finalTitle = book.Title;
            if (!string.IsNullOrWhiteSpace(bookDto.Title))
            {
                finalTitle = bookDto.Title.Trim();
                var titleExists = await CheckDuplicateBookTitleAsync(bookId, finalTitle, ct);
                if (titleExists) return ServiceResponse<BookDto>.Fail("Another book with the same title already exists.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                UpdateBookBasicInfo(book, bookDto, finalTitle);

                // Only replace relations when the caller explicitly sends new IDs.
                if (bookDto.TagIds != null) UpdateBookTags(book, bookDto);
                if (bookDto.AuthorIds != null) UpdateBookAuthors(book, bookDto);

                UpdateBookFiles(book, bookDto);

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return ServiceResponse<BookDto>.Ok(MapToDto(book), "Book updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error updating book {BookId}", bookId);
                return ServiceResponse<BookDto>.Fail($"Error: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteBookAsync(int userId, int bookId, CancellationToken ct = default)
        {
            var book = await _context.Books.FindAsync(new object[] { bookId }, ct);
            if (book == null) return ServiceResponse<bool>.Fail("Book not found.");

            book.DeletedAt = DateTime.UtcNow; // Soft delete — record is kept for audit purposes

            await _context.SaveChangesAsync(ct);
            return ServiceResponse<bool>.Ok(true, "Book moved to trash successfully.");
        }

        // skip this method for now , i added the function to convert the books from plan to another and the same if it was in free plan to premuim plan and reverse too
        public async Task<ServiceResponse<bool>> UpdateMonetizationStatusAsync(int bookId, bool isFree, CancellationToken ct = default)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, ct);
            if (book == null) return ServiceResponse<bool>.Fail("Book not found.");

            book.IsFree = isFree;

            // Logic to move between Free/Paid plans could go here

            await _context.SaveChangesAsync(ct);
            return ServiceResponse<bool>.Ok(true, $"Book is now {(isFree ? "Free" : "Premium")}.");
        }

        public async Task<ServiceResponse<PaginatedResult<BookDto>>> SearchBooksAsync(BookSearchFilterDto filter, PaginationDto dto, CancellationToken ct = default)
        {
            var query = GetBaseBookNotDeletedQuery();

            query = ApplyBookFilters(query, filter);

            query = ApplyBookSorting(query, filter.SortBy);

            var totalRecords = await query.CountAsync(ct);

            var items = await ProjectToDto(query)
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync(ct);

            return ServiceResponse<PaginatedResult<BookDto>>.Ok(new PaginatedResult<BookDto>
            {
                Items = items,
                TotalCount = totalRecords,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize
            });
        }

        public async Task<ServiceResponse<PaginatedResult<BookDto>>> GetBooksByAuthorAsync(int authorId, PaginationDto dto, CancellationToken ct = default)
        {
            if (authorId <= 0)
                return ServiceResponse<PaginatedResult<BookDto>>.Fail("Invalid author ID.");

            var query = GetBooksByAuthorQuery(authorId);

            dto.PageNumber = CheckPageNumber(dto.PageNumber);
            dto.PageSize = CheckPageSize(dto.PageSize);


            var totalRecords = await query.CountAsync(ct);


            var books = await GetBookDtoByAuthor(query, dto.PageNumber, dto.PageSize, ct);

            return ServiceResponse<PaginatedResult<BookDto>>.Ok(
                new PaginatedResult<BookDto> { Items = books, TotalCount = totalRecords, PageNumber = dto.PageNumber, PageSize = dto.PageSize }
            );
        }

        private static ServiceResponse<bool> ValidateBookCreate(BookCreateDto bookDto)
        {
            if (bookDto is null)
                return ServiceResponse<bool>.Fail("Book data is required.");

            if (string.IsNullOrWhiteSpace(bookDto.Title))
                return ServiceResponse<bool>.Fail("Title is required.");

            if (string.IsNullOrWhiteSpace(bookDto.PdfUrl) && string.IsNullOrWhiteSpace(bookDto.AudioUrl))
                return ServiceResponse<bool>.Fail("A book file URL is required.");

            return ServiceResponse<bool>.Ok(true, "Validation passed.");
        }
        private async Task<bool> CheckDuplicateBookTitleOnCreateAsync(string normalizedTitle, CancellationToken ct = default)
        {
            return await _context.Books
                .AsNoTracking()
                .AnyAsync(b => b.Title == normalizedTitle && b.DeletedAt == null, ct);
        }
        private static Book CreateBookEntity(BookCreateDto bookDto, string normalizedTitle)
        {
            return new Book
            {
                Title = normalizedTitle,
                CoverImageUrl = bookDto.CoverImageUrl,
                IsFree = bookDto.IsFree,
                PublishedAt = bookDto.PublishDate,
                CreatedAt = DateTime.UtcNow,
                Description = bookDto.Description,
                RequiredRank = bookDto.Rank
            };
        }
        private void AddBookFile(Book book, BookCreateDto bookDto)
        {

            var filesToProcess = new List<(string? Url, string Type)>
             {
                 (bookDto.PdfUrl, "pdf"),
                 (bookDto.AudioUrl, "mp3")
             };

            foreach (var file in filesToProcess.Where(f => !string.IsNullOrEmpty(f.Url)))
            {

                var fileInfo = ParseSupabaseUrl(file.Url!);

                var bookFile = new BookFile
                {
                    StorageKey = fileInfo.StorageKey,
                    FileFormat = fileInfo.Format,
                    CdnUrl = file.Url,
                    PageCount = fileInfo.Format == "pdf" ? bookDto.PageCount : null,
                    AudioDuration = fileInfo.Format == "mp3" ? (int?)bookDto.AudioDuration?.TotalSeconds : null,
                    CreatedAt = DateTime.UtcNow,
                    Book = book,
                    FileSizeMb = bookDto.FileSize ?? 0
                };
                if (fileInfo.Format == "mp3" && bookDto.NarratorIds != null)
                {
                    foreach (var narratorId in bookDto.NarratorIds.Distinct())
                    {
                        bookFile.BookFileNarrators.Add(new BookFileNarrator
                        {
                            NarratorId = narratorId
                        });
                    }
                }
                book.Files.Add(bookFile);
            }
        }
        private static void AddBookTags(Book book, BookCreateDto bookDto)
        {
            if (bookDto.TagIds is null)
                return;

            foreach (var tagId in bookDto.TagIds.Distinct())
                book.BookTags.Add(new BookTag { TagId = tagId });
        }
        private static void AddBookAuthors(Book book, BookCreateDto bookDto)
        {
            if (bookDto.AuthorIds is null)
                return;

            foreach (var authorId in bookDto.AuthorIds.Distinct())
                book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }
        /*        private async Task<SubscriptionPlan?> GetPlanForBookMonetizationAsync(bool isFree, CancellationToken ct = default)
                {
                    return await _context.SubscriptionPlans
                        .FirstOrDefaultAsync(p => p.IsActive && (isFree ? p.Price == 0 : p.Price > 0), ct);
                }
        */       /* private void AddBookToPlan(int bookId, int planId)
                {
                    _context.PlanBooks.Add(new PlanBook
                    {
                        BookId = bookId,
                        PlanId = planId
                    });
                }*/
        private IQueryable<Book> GetBaseBookNotDeletedQuery()
        {
            return _context.Books
                .AsNoTracking()
                .Where(b => b.DeletedAt == null);
        }
        private async Task<List<BookDto>> GetBookDtoDescByCreate(IQueryable<Book> query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            return await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    CoverImageUrl = b.CoverImageUrl,
                    IsFree = b.IsFree,
                    PublishedAt = b.PublishedAt
                })
                .ToListAsync(ct);
        }
        private int CheckPageSize(int pageSize)
        {
            const int maxPageSize = 100;
            return pageSize <= 0 ? 10 : Math.Min(pageSize, maxPageSize);
        }
        private int CheckPageNumber(int pageNumber)
        {
            return pageNumber < 1 ? 1 : pageNumber;
        }
        private IQueryable<Book> FilterByIsFree(IQueryable<Book> query, BookSearchFilterDto filter)
        {
            return query.Where(b => b.IsFree == filter.IsFree.Value);
        }
        private IQueryable<Book> FilterSearchTerm(IQueryable<Book> query, BookSearchFilterDto filter)
        {
            return query.Where(b => EF.Functions.Like(b.Title, $"%{filter.SearchTerm}%"));
        }
        private async Task<List<BookDto>> GetBookDtoByAuthor(IQueryable<Book> query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            return await query
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)
                 .Select(b => new BookDto
                 {
                     Id = b.Id,
                     Title = b.Title,
                     CoverImageUrl = b.CoverImageUrl,
                     IsFree = b.IsFree,
                     PublishedAt = b.PublishedAt,
                     RequiredRank = b.RequiredRank,
                 })
                 .ToListAsync(ct);
        }
        private IQueryable<Book> GetBooksByAuthorQuery(int authorId)
        {
            return _context.BookAuthors
                .Where(ba => ba.AuthorId == authorId && ba.Book.DeletedAt == null)
                .Select(ba => ba.Book)
                .Distinct();
        }
        private static ServiceResponse<bool> ValidateBookUpdate(BookUpdateDto bookDto, int bookId)
        {
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid book ID.");

            if (bookDto is null)
                return ServiceResponse<bool>.Fail("Book data is required.");

            if (string.IsNullOrWhiteSpace(bookDto.Title))
                return ServiceResponse<bool>.Fail("Title is required.");

            return ServiceResponse<bool>.Ok(true, "Validation passed.");
        }
        private async Task<Book?> GetBookForUpdateAsync(int bookId, CancellationToken ct = default)
        {
            return await _context.Books
                .Include(b => b.BookTags)
                .Include(b => b.BookAuthors)
                .Include(b => b.Files)
                .FirstOrDefaultAsync(b => b.Id == bookId && b.DeletedAt == null, ct);
        }
        private async Task<Book?> GetBookWithDetailsByIdAsync(int bookId, CancellationToken ct = default)
        {
            return await _context.Books
                .AsNoTracking()
                .Include(b => b.Files)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)

                .FirstOrDefaultAsync(b => b.Id == bookId && b.DeletedAt == null, ct);
        }
        private async Task<bool> CheckDuplicateBookTitleAsync(int bookId, string normalizedTitle, CancellationToken ct = default)
        {
            return await _context.Books
                .AsNoTracking()
                .AnyAsync(b => b.Id != bookId && b.Title == normalizedTitle && b.DeletedAt == null, ct);
        }
        private static void UpdateBookBasicInfo(Book book, BookUpdateDto bookDto, string normalizedTitle)
        {
            book.Title = !string.IsNullOrWhiteSpace(bookDto.Title) ? bookDto.Title : book.Title;
            book.Description = !string.IsNullOrWhiteSpace(bookDto.Description) ? bookDto.Description : book.Description;
            book.CoverImageUrl = !string.IsNullOrWhiteSpace(bookDto.CoverImageUrl) ? bookDto.CoverImageUrl : book.CoverImageUrl;
            book.IsFree = bookDto.IsFree ?? book.IsFree;
            book.PublishedAt = bookDto.PublishDate ?? book.PublishedAt;
            book.RequiredRank = bookDto.Rank ?? book.RequiredRank;

            book.UpdatedAt = DateTime.UtcNow;
        }
        private void UpdateBookTags(Book book, BookUpdateDto bookDto)
        {
            if (bookDto.TagIds == null) return;

            _context.BookTags.RemoveRange(book.BookTags);

            if (bookDto.TagIds is null)
                return;

            foreach (var tagId in bookDto.TagIds.Distinct())
                book.BookTags.Add(new BookTag { TagId = tagId });
        }
        private void UpdateBookAuthors(Book book, BookUpdateDto bookDto)
        {
            if (bookDto.AuthorIds == null) return;
            _context.BookAuthors.RemoveRange(book.BookAuthors);

            if (bookDto.AuthorIds is null)
                return;

            foreach (var authorId in bookDto.AuthorIds.Distinct())
                book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }
        private void UpdateBookFiles(Book book, BookUpdateDto bookDto)
        {
            var files = book.Files.ToList();

            UpdateExistingFiles(files, bookDto);
            AddPdfFileIfNotExists(book, files, bookDto);
            AddAudioFileIfNotExists(book, files, bookDto);
        }
        private static void UpdateExistingFiles(List<BookFile> files, BookUpdateDto bookDto)
        {
            foreach (var file in files)
            {
                if (file.FileFormat == "pdf" && !string.IsNullOrWhiteSpace(bookDto.PdfUrl))
                {
                    file.CdnUrl = bookDto.PdfUrl;
                    file.AudioDuration = 0;
                }
                else if (file.FileFormat == "mp3" && !string.IsNullOrWhiteSpace(bookDto.AudioUrl))
                {
                    file.CdnUrl = bookDto.AudioUrl;
                    file.AudioDuration = (int?)bookDto.AudioDuration?.TotalSeconds ?? 0;
                }
            }
        }
        private static void AddPdfFileIfNotExists(Book book, List<BookFile> files, BookUpdateDto bookDto)
        {
            var hasPdfFile = files.Any(f => f.FileFormat == "pdf");

            if (!hasPdfFile && !string.IsNullOrWhiteSpace(bookDto.PdfUrl))
            {
                book.Files.Add(new BookFile
                {
                    FileFormat = "pdf",
                    CdnUrl = bookDto.PdfUrl,
                    AudioDuration = 0
                });
            }
        }
        private static void AddAudioFileIfNotExists(Book book, List<BookFile> files, BookUpdateDto bookDto)
        {
            var hasAudioFile = files.Any(f => f.FileFormat == "mp3");

            if (!hasAudioFile && !string.IsNullOrWhiteSpace(bookDto.AudioUrl))
            {
                book.Files.Add(new BookFile
                {
                    FileFormat = "mp3",
                    CdnUrl = bookDto.AudioUrl,
                    AudioDuration = (int?)bookDto.AudioDuration?.TotalSeconds ?? 0
                });
            }
        }
        private static BookDto MapToDto(Book book)
        {
            // Build a dictionary keyed by format so each collection is scanned only once (O(1) lookups).
            var filesByFormat = book.Files.ToDictionary(f => f.FileFormat, f => f);

            filesByFormat.TryGetValue(AppConstants.FileFormat.Pdf, out var pdfFile);
            filesByFormat.TryGetValue(AppConstants.FileFormat.Mp3, out var audioFile);

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                CoverImageUrl = book.CoverImageUrl,
                IsFree = book.IsFree,
                RequiredRank = book.RequiredRank,
                PublishedAt = book.PublishedAt,

                PdfUrl = pdfFile?.CdnUrl,
                PageCount = pdfFile?.PageCount,

                AudioUrl = audioFile?.CdnUrl,
                AudioDuration = audioFile?.AudioDuration is int duration
                    ? TimeSpan.FromSeconds(duration)
                    : null
            };
        }
        private IQueryable<BookDto> ProjectToDto(IQueryable<Book> query)
        {
            return query.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                CoverImageUrl = b.CoverImageUrl,
                IsFree = b.IsFree,
                RequiredRank = b.RequiredRank,
                PublishedAt = b.PublishedAt
            });
        }
        private (string StorageKey, string Format) ParseSupabaseUrl(string url)
        {
            // 1. استخراج الـ StorageKey (كل ما بعد /public/)
            var storageKeyRaw = url.Split("/public/").LastOrDefault() ?? string.Empty;
            var storageKey = Uri.UnescapeDataString(storageKeyRaw);

            // 2. استخراج الـ Format (الامتداد بعد آخر نقطة)
            // Path.GetExtension بيرجع ".pdf" فبنعمل Trim للـ "."
            var extension = Path.GetExtension(storageKey).TrimStart('.').ToLower();

            // لو الامتداد مش موجود أو غريب، ممكن نحط default
            var format = string.IsNullOrEmpty(extension) ? "unknown" : extension;

            return (storageKey, format);
        }
        private async Task<int> GetUserEffectiveRankAsync(int? userId, CancellationToken ct)
        {
            return await _context.UserSubscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId &&
                            s.Status == AppConstants.SubscriptionStatus.Active &&
                            s.EndDate > DateTime.UtcNow)
                .Select(s => s.Plan.Rank)
                .OrderByDescending(r => r)
                .FirstOrDefaultAsync(ct);
        }
        private async Task<Tag?> GetTagById(int tagId, CancellationToken ct = default)
        {
            return await _context.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tagId && !t.IsDeleted, ct);
        }
        private async Task<List<BookDto>> FetchBooksByTagAsync(int tagId, CancellationToken ct)
        {
            return await _context.BookTags
                              .Where(bt => bt.TagId == tagId && bt.Book.DeletedAt == null)
                              .Select(bt => new BookDto
                              {
                                  Id = bt.Book.Id,
                                  Title = bt.Book.Title,
                                  CoverImageUrl = bt.Book.CoverImageUrl,
                                  IsFree = bt.Book.IsFree,
                                  PublishedAt = bt.Book.PublishedAt
                              })
                              .ToListAsync(ct);
        }
        private IQueryable<Book> ApplyBookFilters(IQueryable<Book> query, BookSearchFilterDto filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = $"%{filter.SearchTerm}%";
                query = query.Where(b => EF.Functions.Like(b.Title, term) || EF.Functions.Like(b.Description, term));
            }

            if (filter.AuthorId.HasValue)
                query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == filter.AuthorId.Value));

            if (!string.IsNullOrWhiteSpace(filter.FileFormat))
                query = query.Where(b => b.Files.Any(f => f.FileFormat.ToLower() == filter.FileFormat.ToLower()));

            if (filter.IsFree.HasValue)
                query = query.Where(b => b.IsFree == filter.IsFree.Value);

            if (filter.PublishedFrom.HasValue)
                query = query.Where(b => b.CreatedAt >= filter.PublishedFrom.Value);

            if (filter.PublishedTo.HasValue)
                query = query.Where(b => b.CreatedAt <= filter.PublishedTo.Value);

            if (filter.TagIds != null && filter.TagIds.Any())
                query = query.Where(b => b.BookTags.Any(bt => filter.TagIds.Contains(bt.TagId)));

            return query;
        }
        private IQueryable<Book> ApplyBookSorting(IQueryable<Book> query, string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "alphabetical" => query.OrderBy(b => b.Title).ThenBy(b => b.Id),

                "popular" => query.OrderByDescending(b => b.Feedbacks.Any()
                                ? b.Feedbacks.Average(f => f.Rating)
                                : 0).ThenByDescending(b => b.Id),

                "newest" => query.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.Id),

                _ => query.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.Id)
            };
        }
        private async Task<int> GetCountOfNarratorIds(BookCreateDto bookDto, CancellationToken ct)
        {
            return await _context.Narrators
                     .CountAsync(n => bookDto.NarratorIds.Contains(n.Id) && n.DeletedAt == null, ct);
        }
    }
}
