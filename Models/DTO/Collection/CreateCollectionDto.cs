using Bookify.Models.DTO.Tag;

namespace Bookify.Models.DTO.Collection
{
    public class CreateCollectionDto:ITag
    {
        public string Name { get; set; }
    }
}
