namespace Bookify.Models.DTO.Narrator
{
    public interface INarrator
    {
        string Name { get; set; }
        string? Bio { get; set; }
        string? ImageUrl { get; set; }
    }
}
