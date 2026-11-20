using System.ComponentModel.DataAnnotations;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class ProjectSummaryDTO
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        public ProjectStatus Status { get; set; }
        public ProjectValidationType ValidationType { get; set; }
        public required IdNameDTO Manager { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<IdNameDTO> Systems { get; set; } = [];
        public List<IdNameDTO> Locations { get; set; } = [];
    }
}
