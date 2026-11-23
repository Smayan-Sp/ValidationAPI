using System.ComponentModel.DataAnnotations;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models
{
    public class Project
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        public List<Location> Locations { get; set; } = [];
        public List<ComputerSystem> Systems { get; set; } = []; 
        public List<ProjectDocument> ProjectDocuments { get; set; } = [];
        public ProjectValidationType ValidationType { get; set; }
        public Employee Manager { get; set; } = null!;
        public int ManagerId { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public ProjectStatus Status { get; set; } 

        
    }
}
