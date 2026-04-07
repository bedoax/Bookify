using Bookify.Data;
using Bookify.Models.DTO.Tag;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;
        public TagService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<TagDto>> CreateTagAsync(int userId, string tagName, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<TagDto>.Fail("Invalid User Id");

            if (string.IsNullOrWhiteSpace(tagName))

                return ServiceResponse<TagDto>.Fail("Tag name is empty");

            var istagExist = await TagExistsAsync(tagName, ct);
            if (istagExist)
                return ServiceResponse<TagDto>.Fail("the tag name is exist");

            var tag = CreateTag(tagName.Trim());
            await _context.Tags.AddAsync(tag, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<TagDto>.Ok(MapToDto(tag), "Tag created");
        }

        public async Task<ServiceResponse<bool>> DeleteTagAsync(int userId, int tagId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid User Id");

            if (tagId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Tag Id");

            var tag = await _context.Tags
                .Include(t => t.BookTags)
                .FirstOrDefaultAsync(t => t.Id == tagId && !t.IsDeleted, ct);

            if (tag == null)
                return ServiceResponse<bool>.Fail("Tag not found");

            // if tag connected with books then use soft delete
            if (tag.BookTags.Any())
            {
                tag.IsDeleted = true;
            }
            else
            {
                // for hard delete if tag not connected with books 
                _context.Tags.Remove(tag);
            }

            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, "Tag deleted successfully");
        }


        public async Task<ServiceResponse<IEnumerable<TagDto>>> GetAllTagsAsync(CancellationToken ct = default)
        {
            var tags = await FetchTagsAsync(ct);

            return ServiceResponse<IEnumerable<TagDto>>.Ok(tags);
        }

        public async Task<ServiceResponse<TagDto>> GetTagByIdAsync(int tagId, CancellationToken ct = default)
        {
            if (tagId <= 0)
                return ServiceResponse<TagDto>.Fail("Invalid Tag Id");

            var tag = await GetTagById(tagId, ct);
            if (tag == null)
                return ServiceResponse<TagDto>.Fail("tag is not found");

            return ServiceResponse<TagDto>.Ok(MapToDto(tag), "Tag had Found");

        }


        public async Task<ServiceResponse<TagDto>> UpdateTagAsync(int userId, int tagId, string newName, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<TagDto>.Fail("Invalid User Id");

            if (string.IsNullOrWhiteSpace(newName))
                return ServiceResponse<TagDto>.Fail("Tag name is empty");

            if (tagId <= 0)
                return ServiceResponse<TagDto>.Fail("Invalid Tag Id");

            var tag = await GetTagById(tagId, ct);
            if (tag == null)
                return ServiceResponse<TagDto>.Fail("tag is not found");
            var isExist = await TagExistsAsync(newName, ct);
            if (isExist)
                return ServiceResponse<TagDto>.Fail("the tag name is exist");
            await _context.Tags
            .Where(t => t.Id == tagId)
              .ExecuteUpdateAsync(setters =>
                setters.SetProperty(t => t.Name, newName.Trim()), ct);
            await _context.SaveChangesAsync(ct);
            return ServiceResponse<TagDto>.Ok(MapToDto(tag), "Updated new name of tag");
        }
        private async Task<bool> TagExistsAsync(string tagname, CancellationToken ct)
        {
            // && x.IsDeleted == false to get the only not deleteed tags for creatting  or esle
            return await _context.Tags
                            .AnyAsync(x => x.Name.ToLower() == tagname.Trim().ToLower(), ct);
        }
        private Tag CreateTag(string tagName)
        {
            return new Tag
            {
                Name = tagName,
            };
        }
        private async Task<IEnumerable<TagDto>> FetchTagsAsync(CancellationToken ct)
        {
            return await _context.Tags.Select(x => new TagDto { Id = x.Id, Name = x.Name }).ToListAsync(ct);
        }
        private async Task<Tag?> GetTagById(int tagId, CancellationToken ct)
        {
            return await _context.Tags.FirstOrDefaultAsync(x => x.Id == tagId, ct);
        }
        private static TagDto MapToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }

    }
}
