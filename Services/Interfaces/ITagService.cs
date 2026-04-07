using Bookify.Models.DTO.Tag;
using Bookify.Services.Implementations;

namespace Bookify.Services.Interfaces
{
    public interface ITagService
    {
        Task<ServiceResponse<TagDto>> CreateTagAsync(int userId, string tagName, CancellationToken ct = default);
        Task<ServiceResponse<TagDto>> GetTagByIdAsync(int tagId, CancellationToken ct = default);
        Task<ServiceResponse<TagDto>> UpdateTagAsync(int userId, int tagId, string newName, CancellationToken ct = default);
        Task<ServiceResponse<bool>> DeleteTagAsync(int userId, int tagId, CancellationToken ct = default);
        Task<ServiceResponse<IEnumerable<TagDto>>> GetAllTagsAsync(CancellationToken ct = default);
    }
}
