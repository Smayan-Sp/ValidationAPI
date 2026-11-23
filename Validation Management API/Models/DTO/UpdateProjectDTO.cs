using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class UpdateProjectDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public int? ManagerId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public List<int>? SystemIds { get; set; }
        public List<int>? LocationIds { get; set; }
    }
}
