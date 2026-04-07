
    using global::Bookify.Data;
    using global::Bookify.Models.DTO.Narrator;
    using global::Bookify.Models.DTO.Narrto;
    using global::Bookify.Models.Entities;
    using global::Bookify.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;

    namespace Bookify.Services.Implementations
    {
        public class NarratorService : INarratorService
        {
            private readonly AppDbContext _context;
            public NarratorService(AppDbContext context)
            {
                _context = context;
            }

            // Create a new narrator
            public async Task<ServiceResponse<NarratorDto>> CreateNarratorAsync(NarratorCreateDto dto, CancellationToken ct = default)
            {
                var validate = ValidateNarrator(dto);
                if (!validate.Success)
                    return ServiceResponse<NarratorDto>.Fail(validate.Message);

                var isDuplicate = await CheckDuplicateName(dto.Name.Trim(), ct);
                if (isDuplicate)
                    return ServiceResponse<NarratorDto>.Fail("The name already exists");

                var narrator = CreateNarrator(dto);

                await _context.Narrators.AddAsync(narrator, ct);
                await _context.SaveChangesAsync(ct);

                return ServiceResponse<NarratorDto>.Ok(MapToDto(narrator), "Narrator created successfully");
            }

            // Get narrator by Id
            public async Task<ServiceResponse<NarratorDto>> GetNarratorByIdAsync(int id, CancellationToken ct = default)
            {
                if (id <= 0)
                    return ServiceResponse<NarratorDto>.Fail("Invalid Narrator Id");

                var narrator = await _context.Narrators.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);
                if (narrator == null)
                    return ServiceResponse<NarratorDto>.Fail("Narrator not found");

                return ServiceResponse<NarratorDto>.Ok(MapToDto(narrator));
            }

            // Get all narrators with pagination
            public async Task<ServiceResponse<PaginatedResult<NarratorDto>>> GetNarratorsAsync(int pageNumber, int pageSize, CancellationToken ct = default)
            {
                if (pageNumber <= 0 || pageSize <= 0)
                    return ServiceResponse<PaginatedResult<NarratorDto>>.Fail("Invalid page number or page size");

                var narrators = await GetListNarratorsDto(pageNumber, pageSize, ct);

                var result = new PaginatedResult<NarratorDto>
                {
                    Items = narrators,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ServiceResponse<PaginatedResult<NarratorDto>>.Ok(result, "Fetched successfully");
            }

            // Update narrator
            public async Task<ServiceResponse<NarratorDto>> UpdateNarratorAsync(int narratorId, NarratorUpdateDto dto, CancellationToken ct = default)
            {
                if (narratorId <= 0)
                    return ServiceResponse<NarratorDto>.Fail("Invalid Narrator Id");

                var narrator = await GetNarratorById(narratorId, ct);
                if (narrator == null)
                    return ServiceResponse<NarratorDto>.Fail("Narrator not found");

                var validate = ValidateNarrator(dto);
                if (!validate.Success)
                    return ServiceResponse<NarratorDto>.Fail(validate.Message);

                narrator.Name = dto.Name.Trim();
                narrator.Bio = dto.Bio;
                narrator.ImageUrl = dto.ImageUrl;
                narrator.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);

                return ServiceResponse<NarratorDto>.Ok(MapToDto(narrator), "Updated successfully");
            }

            // Delete narrator
            public async Task<ServiceResponse<bool>> DeleteNarratorAsync(int narratorId, CancellationToken ct = default)
            {
                if (narratorId <= 0)
                    return ServiceResponse<bool>.Fail("Invalid Narrator Id");

                var narrator = await GetNarratorById(narratorId, ct);
                if (narrator == null)
                    return ServiceResponse<bool>.Fail("Narrator not found");

                narrator.DeletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(ct);

                return ServiceResponse<bool>.Ok(true, "Deleted successfully");
            }

        
            private ServiceResponse<bool> ValidateNarrator(INarrator dto)
            {
                if (dto == null) return ServiceResponse<bool>.Fail("DTO is null");
                if (string.IsNullOrWhiteSpace(dto.Name)) return ServiceResponse<bool>.Fail("Name is required");
                return ServiceResponse<bool>.Ok(true);
            }

            private async Task<bool> CheckDuplicateName(string name, CancellationToken ct)
            {
            return await _context.Narrators.AnyAsync(n => n.Name.ToLower() == name.ToLower(), ct);

            }

        private static  NarratorDto MapToDto(Narrator narrator)
            {
                return new NarratorDto
                {
                    Id = narrator.Id,
                    Name = narrator.Name,
                    Bio = narrator.Bio,
                    ImageUrl = narrator.ImageUrl
                };
            }
            private async Task<Narrator?> GetNarratorById(int narratorId, CancellationToken ct)
            {
                return await _context.Narrators.FirstOrDefaultAsync(n => n.Id == narratorId, ct);
            }
            private async Task<List<NarratorDto>> GetListNarratorsDto(int pageNumber, int pageSize, CancellationToken ct)
            {
                return await _context.Narrators.AsNoTracking()
                    .Where(n => n.DeletedAt == null)
                    .Select(n => MapToDto(n))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);
            }
            private Narrator CreateNarrator(NarratorCreateDto dto)
            {
               return new Narrator
                {
                    Name = dto.Name.Trim(),
                    Bio = dto.Bio,
                    ImageUrl = dto.ImageUrl,
                    CreatedAt = DateTime.UtcNow
               };
            }
        }
   
    }
