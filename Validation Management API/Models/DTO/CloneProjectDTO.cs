using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class CloneProjectDTO
    {
        [MaxLength(100)]
        public required string Name { get; set; }
        public int ManagerId { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }
} 
