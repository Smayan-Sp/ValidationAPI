using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class ProjectContributorDTO
    {
        public int EmployeeId { get; set; }
        public required string Name { get; set; }
        public TaskType Role { get; set; }
        public required string DocumentName { get; set; }
    }
}
