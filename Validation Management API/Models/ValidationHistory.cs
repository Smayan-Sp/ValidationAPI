using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models
{
    public class ValidationHistory
    {
        public int Id { get; set; }
        public ComputerSystem ComputerSystem { get; set; } = null!;
        public int ComputerSystemId { get; set; }
        public ValidationStatus Status { get; set; } = ValidationStatus.Pending;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Project? Project { get; set; }
        public int? ProjectId { get; set; }

    }
}
