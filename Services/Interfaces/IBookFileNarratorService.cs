using Bookify.Models.DTO.Narrator;
using Bookify.Models.DTO.Narrto;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface IBookFileNarratorService
    {
        Task<ServiceResponse<bool>> AddNarratorToBookFileAsync(int bookFileId, int narratorId, CancellationToken ct = default);
        Task<ServiceResponse<bool>> RemoveNarratorFromBookFileAsync(int bookFileId, int narratorId, CancellationToken ct = default);
        Task<ServiceResponse<List<NarratorDto>>> GetNarratorsByBookFileAsync(int bookFileId, CancellationToken ct = default);
        Task<ServiceResponse<List<BookFileNarratorDto>>> GetBookFilesByNarratorAsync(int narratorId, CancellationToken ct = default);
    }
}
