namespace Validation_Management_API.Models.DTO
{
    public class PlanContributorDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsMandatory { get; set; }
    }
}
