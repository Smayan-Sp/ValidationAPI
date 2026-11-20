using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models
{
    public class Attachment
    {
        public int Id {  get; set; }
        [MaxLength(255)]
        public required string OriginalName { get; set; }
        public required long ContentSize { get; set; }
        [MaxLength(255)]
        public required string ContentType { get; set; }
        public required byte[] Content { get; set; }

        public ComputerSystem Computer { get; set; } = null!;
        public int ComputerId { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
