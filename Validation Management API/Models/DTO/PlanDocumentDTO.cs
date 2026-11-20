namespace Validation_Management_API.Models.DTO
{
    public class PlanDocumentDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public List<IdNameDTO> DependsOn { get; set; } = [];
        public List<PlanTaskDTO> Tasks { get; set; } = [];
    }
}
