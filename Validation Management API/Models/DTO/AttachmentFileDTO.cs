namespace Validation_Management_API.Models.DTO
{
    public class AttachmentFileDTO
    {
        public required string OriginalName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] Content { get; set; }
    }
}
