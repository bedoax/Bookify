using Bookify.Data;
using Bookify.Models.DTO.Author;
using Bookify.Models.Entities;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Bookify.Services.Implementations
{
    public class AuthorService : IAuthorService
    {
        private AppDbContext _context;
        public AuthorService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<AuthorDto>> CreateAuthorAsync(int userId, AuthorCreateDto authorDto, CancellationToken ct = default)
        {
            var ValidationAuthor = ValidateAuthor(authorDto);
            if (!ValidationAuthor.Success)
                return ServiceResponse<AuthorDto>.Fail(ValidationAuthor.Message);


            var isDuplicateNmae = await CheckDuplicateNameOfAuthor(authorDto.Name.Trim(), ct);

            if (isDuplicateNmae)
                return ServiceResponse<AuthorDto>.Fail("The name is exist ");

            var author = CreateAuthor(authorDto);

            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync(ct);

            return ServiceResponse<AuthorDto>.Ok(MapToAuthorDtoFromAuthorClass(author), "Data Succsed Added");
        }

        public async Task<ServiceResponse<bool>> DeleteAuthorAsync(int userId, int authorId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid User Id");
            if (authorId <= 0)
                return ServiceResponse<bool>.Fail("Invalid Author Id");
            var author = await GetAuthor(authorId, ct);
            if (author is null)
                return ServiceResponse<bool>.Fail("Author Not Found");

            author.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return ServiceResponse<bool>.Ok(true, $"Had Dealeted at {author.DeletedAt}");
        }

        public async Task<ServiceResponse<AuthorDto>> GetAuthorByIdAsync(int authorId, CancellationToken ct = default)
        {
            if (authorId <= 0)
                return ServiceResponse<AuthorDto>.Fail("Invalid AuthorID");
            var author = await GetAuthorDataByIdAsync(authorId, ct);
            if (author == null)
                return ServiceResponse<AuthorDto>.Fail("Author Not Found");

            var authorDto = MapToAuthorDtoFromAuthorClass(author);
            return ServiceResponse<AuthorDto>.Ok(authorDto, "Found it");
        }

        public async Task<ServiceResponse<PaginatedResult<AuthorDto>>> GetAuthorsAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            if (pageNumber <= 0)
                return ServiceResponse<PaginatedResult<AuthorDto>>.Fail("Invalid Page Number");
            if (pageSize <= 0)
                return ServiceResponse<PaginatedResult<AuthorDto>>.Fail("Invalid Page Size");


            var authorsDto = await GetAllAuthorsDto(pageNumber, pageSize, ct);

            var PaginatedResultAuthorDto = await MapToPaginatedResultAuthorDto(authorsDto, pageNumber, pageSize);

            return ServiceResponse<PaginatedResult<AuthorDto>>.Ok(PaginatedResultAuthorDto, "Data had Fetched Correctly");
        }
        public async Task<ServiceResponse<AuthorDto>> GetAuthorByNameAsync(string authorName, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(authorName))
                return ServiceResponse<AuthorDto>.Fail("Author Name is Empty");

            var authorNameTrim = authorName.Trim().ToLower();
            var author = await GetAuthorByName(authorNameTrim, ct);

            if (author is null)
                return ServiceResponse<AuthorDto>.Fail("Author Not Found");


            var authorDto = MapToAuthorDtoFromAuthorClass(author);

            return ServiceResponse<AuthorDto>.Ok(authorDto, "Data had Fetched Correctly");
        }
        public async Task<ServiceResponse<AuthorDto>> UpdateAuthorAsync(int userId, int authorId, AuthorUpdateDto authorUpdateDto, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<AuthorDto>.Fail("Invalid UserId");
            if (authorId <= 0)
                return ServiceResponse<AuthorDto>.Fail("Invalid AuthorId");
            var author = await GetAuthor(authorId, ct);
            if (author is null)
                return ServiceResponse<AuthorDto>.Fail("Author Not Found");
            var validate = ValidateAuthor(authorUpdateDto);
            if (!validate.Success)
                return ServiceResponse<AuthorDto>.Fail(validate.Message);
            author.Deathdate = authorUpdateDto.Deathdate;
            author.Name = authorUpdateDto.Name;
            author.Bio = authorUpdateDto.Bio;
            author.Birthdate = authorUpdateDto.Birthdate;
            await _context.SaveChangesAsync(ct);
            var authorDto = new AuthorDto
            {
                Bio = authorUpdateDto.Bio,
                Birthdate = authorUpdateDto.Birthdate,
                Deathdate = authorUpdateDto.Deathdate,
                Name = authorUpdateDto.Name,
            };
            return ServiceResponse<AuthorDto>.Ok(authorDto, "Data Had Update It");
        }
        private Author CreateAuthor(AuthorCreateDto authorCreateDto)
        {
            return new Author
            {

                Bio = authorCreateDto.Bio,
                Birthdate = authorCreateDto.Birthdate,
                Name = authorCreateDto.Name,
                Deathdate = authorCreateDto.Deathdate,
                CreatedAt = DateTime.UtcNow,
            };
        }

        private ServiceResponse<bool> ValidateAuthor(IAuthor dto)
        {
            if (dto == null)
                return ServiceResponse<bool>.Fail("dto is null");
            if (string.IsNullOrEmpty(dto.Name))
                return ServiceResponse<bool>.Fail("name is empty please fill it ");

            return ServiceResponse<bool>.Ok(true, "there's no errors");
        }
        private async Task<bool> CheckDuplicateNameOfAuthor(string name, CancellationToken ct)
        {
            return await _context.Authors.AnyAsync(a => a.Name == name, ct);
        }
        private AuthorDto MapToAuthorDtoFromAuthorClass(Author author)
        {
            return new AuthorDto
            {
                Id = author.Id,
                Bio = author.Bio,
                Birthdate = author.Birthdate,
                Deathdate = author.Deathdate,
                Name = author.Name,
            };
        }
        private async Task<Author?> GetAuthorDataByIdAsync(int id, CancellationToken ct)
        {
            return await _context.Authors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.Deathdate == null, ct);
        }
        private async Task<IEnumerable<AuthorDto>> GetAllAuthorsDto(int pageNumber, int pageSize, CancellationToken ct)
        {

            return await _context.Authors.AsNoTracking().Select(x => new AuthorDto
            {
                Id = x.Id,
                Bio = x.Bio,
                Birthdate = x.Birthdate,
                Deathdate = x.Deathdate,
                Name = x.Name,
                DeletedAt = x.DeletedAt,
            }).Where(x => x.DeletedAt == null).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        }
        private async Task<PaginatedResult<AuthorDto>> MapToPaginatedResultAuthorDto(IEnumerable<AuthorDto> item, int pageNumber, int pageSize)

        {
            return new PaginatedResult<AuthorDto>
            {
                Items = item,
                PageSize = pageSize,
                PageNumber = pageNumber,
            };
        }
        private async Task<Author?> GetAuthor(int id, CancellationToken ct)
        {
            return await _context.Authors.FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, ct);
        }
        private async Task<bool> CheckAuthorIsExist(int authorId, CancellationToken ct)
        {
            return await _context.Authors.AnyAsync(x => x.Id == authorId && x.DeletedAt == null, ct);
        }
        private async Task<Author?> GetAuthorByName(string name, CancellationToken ct)
        {
            return await _context.Authors.FirstOrDefaultAsync(a => a.Name == name && a.DeletedAt == null, ct);
        }
    }
}
