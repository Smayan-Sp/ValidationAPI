namespace Validation_Management_API.Models.DTO
{
    public class AttachmentDTO
    {
        public int Id { get; set; }
        public required string OriginalName { get; set; }
        public required string ContentType { get; set; }
        public long ContentSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
