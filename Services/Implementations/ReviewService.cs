using Bookify.Data;
using Bookify.Models.DTO.Pagination;
using Bookify.Models.DTO.Review;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        public ReviewService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<ReviewDto>> CreateReviewAsync(int userId, ReviewCreateDto reviewDto, CancellationToken ct = default)
        {
            var validationReviewCreate = ValidationReviewCreateDto(reviewDto);
            if (!validationReviewCreate.Success)
                return ServiceResponse<ReviewDto>.Fail(validationReviewCreate.Message);

            var user = await GetUser(userId, ct);
            if (user is null)
                return ServiceResponse<ReviewDto>.Fail("user not found");

            var book = await GetBook(reviewDto.BookId, ct);
            if (book is null)
                return ServiceResponse<ReviewDto>.Fail("book not found");

            var isDuplicateReview = await IsReviewExist(userId, reviewDto.BookId, ct);
            if (isDuplicateReview)
                return ServiceResponse<ReviewDto>.Fail("The Review already exists");

            var bookReview = CreateReview(user, reviewDto, book);

            await _context.AddAsync(bookReview, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<ReviewDto>.Ok(MapToReviewDto(bookReview), "Data Had Created Succsifly");
        }

        public async Task<ServiceResponse<bool>> DeleteReviewAsync(int userId, int reviewId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid user ID.");
            if (reviewId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Review Id");
            var isUserExist = await IsUserExist(userId, ct);
            if (!isUserExist)
                return ServiceResponse<bool>.Fail("User Not Found");
            var review = await GetReview(reviewId, ct);
            if (review is null)
                return ServiceResponse<bool>.Fail("Review not Found");
            if (review.UserId != userId)
                return ServiceResponse<bool>.Fail("U don't Have permission To delete it");
            _context.BookReview.Remove(review);
            await _context.SaveChangesAsync(ct);
            return ServiceResponse<bool>.Ok(true, "Review Had Delete it");
        }

        public async Task<ServiceResponse<ReviewDto>> GetReviewByIdAsync(int reviewId, CancellationToken ct = default)
        {
            if (reviewId <= 0)
                return ServiceResponse<ReviewDto>.Fail("Invalid Review Id");
            var review = await GetReview(reviewId, ct);
            if (review is null)
                return ServiceResponse<ReviewDto>.Fail("Review Not Found");
            return ServiceResponse<ReviewDto>.Ok(MapToReviewDto(review), "Data Had Fetched Correctly");

        }

        public async Task<ServiceResponse<PaginatedResult<ReviewDto>>> GetReviewsByBookAsync(int bookId, PaginationDto dto, CancellationToken ct = default)
        {
            if (bookId <= 0)
                return ServiceResponse<PaginatedResult<ReviewDto>>.Fail("Invalid bookId");

            if (dto.PageNumber <= 0)
                return ServiceResponse<PaginatedResult<ReviewDto>>.Fail("Invalid Page Number");
            if (dto.PageSize <= 0)
                return ServiceResponse<PaginatedResult<ReviewDto>>.Fail("Invalid Page Size");
            var isBookExist = await IsBookExist(bookId, ct);
            if (!isBookExist)
                return ServiceResponse<PaginatedResult<ReviewDto>>.Fail("Book not Found");
            var reviews = await GetReviewsAsync(bookId, dto.PageNumber, dto.PageSize, ct);
            return ServiceResponse<PaginatedResult<ReviewDto>>.Ok(reviews, "Data Had Fetched Correctly");
        }

        public async Task<ServiceResponse<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, ReviewUpdateDto reviewDto, CancellationToken ct = default)
        {
            if (reviewDto is null)
                return ServiceResponse<ReviewDto>.Fail("Review data is required.");

            if (userId <= 0)
                return ServiceResponse<ReviewDto>.Fail("Invalid user ID.");
            if (reviewId <= 0)
                return ServiceResponse<ReviewDto>.Fail("Invalid review ID");
            var isUserExist = await IsUserExist(userId, ct);
            if (!isUserExist)
                return ServiceResponse<ReviewDto>.Fail("User Not Found");
            var review = await GetReview(reviewId, ct);
            if (review is null)
                return ServiceResponse<ReviewDto>.Fail("Review not Found");
            if (review.UserId != userId)
                return ServiceResponse<ReviewDto>.Fail("U don't Have permission To Edit it");
            if (reviewDto.Rating <= 0 || reviewDto.Rating > 5)
                return ServiceResponse<ReviewDto>.Fail("Invalid Rating");

            review.Rating = reviewDto.Rating;
            review.ReviewContent = reviewDto.ReviewContent;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return ServiceResponse<ReviewDto>.Ok(MapToReviewDto(review), "Review updated successfully.");
        }
        private ServiceResponse<bool> ValidationReviewCreateDto(ReviewCreateDto ReviewCreateDto)
        {
            if (ReviewCreateDto == null)
                return ServiceResponse<bool>.Fail("Review is empty");
            if (ReviewCreateDto.BookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Book Id");
            if (ReviewCreateDto.Rating <= 0 || ReviewCreateDto.Rating > 5)
                return ServiceResponse<bool>.Fail("Invalid Rating Number");


            return ServiceResponse<bool>.Ok(true);
        }
        private async Task<User?> GetUser(int userId, CancellationToken ct)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        }
        private async Task<Book?> GetBook(int bookId, CancellationToken ct)
        {
            return await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookId, ct);
        }
        private BookReview CreateReview(User user, ReviewCreateDto reviewDto, Book book)
        {
            return new BookReview
            {
                BookId = reviewDto.BookId,
                CreatedAt = DateTime.UtcNow,
                Rating = reviewDto.Rating,
                UserId = user.Id,
                ReviewContent = reviewDto.ReviewContent,
            };
        }
        private async Task<BookReview?> GetReview(int reviewId, CancellationToken ct)
        {
            return await _context.BookReview.FirstOrDefaultAsync(b => b.Id == reviewId, ct);
        }
        private ReviewDto MapToReviewDto(BookReview review)
        {
            return new ReviewDto
            {
                BookId = review.BookId,
                Rating = review.Rating,
                ReviewContent = review.ReviewContent,
                UserId = review.UserId,
            };
        }
        private async Task<PaginatedResult<ReviewDto>> GetReviewsAsync(int bookId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

            var query = _context.BookReview
                        .AsNoTracking()
                        .Where(b => b.BookId == bookId);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new ReviewDto
                {
                    BookId = b.BookId,
                    Rating = b.Rating,
                    ReviewContent = b.ReviewContent,
                    UserId = b.UserId
                })
                .ToListAsync(ct);

            return new PaginatedResult<ReviewDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        }
        private async Task<bool> IsBookExist(int bookId, CancellationToken ct)
        {
            return await _context.Books.AnyAsync(b => b.Id == bookId, ct);
        }
        private async Task<bool> IsReviewExist(int userId, int bookId, CancellationToken ct)
        {
            return await _context.BookReview.AnyAsync(r => r.UserId == userId && r.BookId == bookId, ct);
        }
        private async Task<bool> IsUserExist(int userId, CancellationToken ct)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId, ct);
        }
    }
}
