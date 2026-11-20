namespace Validation_Management_API.Models.DTO
{
    public class PlanDTO
    {
        public int ProjectId { get; set; }
        public required string ProjectName { get; set; }
        public List<PlanDocumentDTO> Documents { get; set; } = [];
    }
}
