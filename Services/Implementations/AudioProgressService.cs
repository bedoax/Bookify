using Bookify.Data;
using Bookify.Models.DTO.Book;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class AudioProgressService : IAudioProgressService
    {
        private readonly AppDbContext _context;
        public AudioProgressService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<AudioProgressDto>> GetAudioProgressAsync(int userId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<AudioProgressDto>.Fail("Invalid User id");
            if (bookId <= 0)
                return ServiceResponse<AudioProgressDto>.Fail("Invalid Book id");


            var AudioProgress = await FetchAudioInformation(userId, bookId, ct);

            if (AudioProgress is null)
                return ServiceResponse<AudioProgressDto>.Fail("BookProgress not found");
            return ServiceResponse<AudioProgressDto>.Ok(AudioProgress);
        }
        public async Task<ServiceResponse<bool>> CreateAudioProgressAsync(int userId, int bookId, TimeSpan progress, CancellationToken ct)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid User id");
            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Book id");

            var newProgress = new UserAudioBookProgress
            {
                UserId = userId,
                BookId = bookId,
                LastListenSecond = (int)progress.TotalSeconds,
                UpdatedAt = DateTime.UtcNow,

            };

            await _context.UserAudioBookProgress.AddAsync(newProgress, ct);
            await _context.SaveChangesAsync(ct);
            return ServiceResponse<bool>.Ok(true, "Book Progress Has Created");
        }
        public async Task<ServiceResponse<bool>> UpdateAudioProgressAsync(int userId, int bookId, TimeSpan progress, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid User id");

            if (bookId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Book id");

            if (progress.TotalSeconds < 0)
                return ServiceResponse<bool>.Fail("Invalid progress");

            var seconds = (int)progress.TotalSeconds;

            try
            {
                var affectedRows = await _context.UserAudioBookProgress
                    .Where(u => u.UserId == userId && u.BookId == bookId && u.LastListenSecond < seconds)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.LastListenSecond, seconds)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct);

                if (affectedRows == 0)
                {
                    var exists = await isUserAudioBookProgressExist(userId, bookId, ct);

                    if (!exists)
                        await CreateAudioProgressAsync(userId, bookId, progress, ct);
                }

                return ServiceResponse<bool>.Ok(true);
            }
            catch
            {
                return ServiceResponse<bool>.Fail("Error updating progress");
            }
        }

        private async Task<AudioProgressDto?> FetchAudioInformation(int userId, int bookId, CancellationToken ct)
        {
            return await _context.Books
                        // if not worked then remove the comment and try again
                        /*                                            .Include(b => b.Files)
                                                                    .Include(b => b.ListenersProgress)*/
                        .Where(b => b.Id == bookId)
                        .Select(b => new
                        {

                            TotalDuration = b.Files.Select(f => (int?)f.AudioDuration).FirstOrDefault() ?? 0,
                            UserProgress = b.ListenersProgress.FirstOrDefault(p => p.UserId == userId)
                        })
                        .Select(temp => new AudioProgressDto
                        {
                            AudioDuration = temp.TotalDuration,
                            CurrentPositionSeconds = temp.UserProgress != null ? (int)temp.UserProgress.LastListenSecond : 0,
                            LastListenedAt = temp.UserProgress != null ? temp.UserProgress.UpdatedAt : null,

                            // حساب النسبة المئوية بدقة مع تجنب القسمة على صفر
                            ProgressPercentage = (temp.TotalDuration > 0 && temp.UserProgress != null)
                                ? Math.Round(((double)temp.UserProgress.LastListenSecond / temp.TotalDuration) * 100, 2)
                                : 0
                        })
                        .FirstOrDefaultAsync(ct);
        }
        private async Task<UserAudioBookProgress?> FetchUserBookProgressAsync(int userId, int bookId, CancellationToken ct)
        {
            return await _context.UserAudioBookProgress.FirstOrDefaultAsync(u => u.UserId == userId && u.BookId == bookId, ct);
        }
        private async Task<bool> isUserAudioBookProgressExist(int userId, int bookId, CancellationToken ct)
        {
            return await _context.UserAudioBookProgress
                .AnyAsync(x => x.UserId == userId && x.BookId == bookId, ct);
        }
    }
}
