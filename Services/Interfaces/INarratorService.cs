using Bookify.Models.DTO.Narrator;
using Bookify.Models.DTO.Narrto;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface INarratorService
    {
        Task<ServiceResponse<NarratorDto>> CreateNarratorAsync(NarratorCreateDto dto, CancellationToken ct = default);
        Task<ServiceResponse<NarratorDto>> GetNarratorByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResponse<PaginatedResult<NarratorDto>>> GetNarratorsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<ServiceResponse<NarratorDto>> UpdateNarratorAsync(int narratorId, NarratorUpdateDto dto, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteNarratorAsync(int narratorId, CancellationToken ct = default);
    }
}
