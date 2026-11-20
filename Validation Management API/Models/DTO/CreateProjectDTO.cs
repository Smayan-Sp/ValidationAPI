using System.ComponentModel.DataAnnotations;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class CreateProjectDTO
    {
        [MaxLength(100)]
        public required string Name { get; set; }
        public int ManagerId { get; set; }
        public ProjectValidationType ValidationType { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public List<int> SystemIds { get; set; } = [];
        public List<int> LocationIds { get; set; } = [];
    }
}
