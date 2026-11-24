using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class PlanTaskDTO
    {
        public int Id { get; set; }
        public TaskType Type { get; set; }
        public int Number { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public List<PlanContributorDTO> Contributors { get; set; } = [];
    }
}
