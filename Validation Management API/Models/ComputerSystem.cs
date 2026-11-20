using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models
{
    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Code), IsUnique = true)]
    public class ComputerSystem
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Code {  get; set; }
        public Department Department { get; set; } = null!;
        public int DepartmentId { get; set; }
        public  Location Location { get; set; } = null!;
        public int LocationId { get; set; }
        public  Employee Owner { get; set; } = null!;
        public int OwnerId { get; set; }
        public List<ValidationHistory>? History { get; set; } = [];
        public bool IsActive { get; set; } = true;
        public List<Attachment>? Attachments { get; set; } = [];
        public List<Project>? Projects { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
