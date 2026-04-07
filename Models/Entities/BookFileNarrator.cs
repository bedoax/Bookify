namespace Bookify.Models.Entities
{
    public class BookFileNarrator
    {
        public int BookFileId { get; set; }
        public BookFile BookFile { get; set; } = null!;

        public int NarratorId { get; set; }
        public Narrator Narrator { get; set; } = null!;
    }
}
