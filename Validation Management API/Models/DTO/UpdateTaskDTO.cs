using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class UpdateTaskDTO
    {
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public ProjectTaskStatus? Status { get; set; }
        public List<TaskContributorInputDTO>? Contributors { get; set; }
    }
}
