using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class ValidationHistoryDTO
    {
        public int Id { get; set; }
        public ValidationStatus Status { get; set; } = ValidationStatus.Pending;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Project { get; set; }
    }
}
