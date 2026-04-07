using Bookify.Models.DTO.Book;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Bookify.Models.DTO.Collection
{
    public class CollectionDto
    {
        public int Id { get; set; }
        // Essential for the "Gallery" view: "12 Books"
        public string Name { get; set; }
        public int BookCount { get; set; }


        // --- The "Dynamic" Content ---
        // For 'GetCollectionsAsync' (List), this might be empty or just the top 3 covers.
        // For 'GetCollectionByIdAsync' (Detail), this contains the full book list.
        public IEnumerable<BookDto> Books { get; set; }

    }
}
