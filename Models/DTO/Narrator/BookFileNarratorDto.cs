namespace Bookify.Models.DTO.Narrator
{
    public class BookFileNarratorDto
    {
        public int BookFileId { get; set; }
        public string FileFormat { get; set; } = string.Empty;
        public decimal? AudioDuration { get; set; }
        public string BookTitle { get; set; } = string.Empty;  // اسم الكتاب
        public int NarratorId { get; set; }
        public string NarratorName { get; set; } = string.Empty;
    }


}
