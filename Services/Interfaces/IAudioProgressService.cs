using Bookify.Models.DTO.Book;
using Bookify.Services.Implementations;
using System.Net;

namespace Bookify.Services.Interfaces
{
    public interface IAudioProgressService
    {
        Task<ServiceResponse<bool>> UpdateAudioProgressAsync(int userId, int bookId, TimeSpan progress, CancellationToken ct = default);
        Task<ServiceResponse<AudioProgressDto>> GetAudioProgressAsync(int userId, int bookId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> CreateAudioProgressAsync(int userId, int bookId, TimeSpan progress, CancellationToken ct = default);
    }
}
