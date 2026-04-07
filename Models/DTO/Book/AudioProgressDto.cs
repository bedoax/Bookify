using System.Text.Json.Serialization.Metadata;

namespace Bookify.Models.DTO.Book
{
    public class AudioProgressDto
    {
        // Decimal allows for sub-second precision (great for resuming mid-sentence)
        public decimal CurrentPositionSeconds { get; set; }

        // Measured in seconds for calculation consistency
        public int AudioDuration { get; set; }

        // (CurrentPositionSeconds / AudioDuration) * 100 in service layer for client convenience
        public double ProgressPercentage { get; set; }

        public DateTime? LastListenedAt { get; set; }

    }
}
