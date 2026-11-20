using Microsoft.EntityFrameworkCore;

namespace Validation_Management_API.Models
{
    [Index(nameof(ProjectTaskId), nameof(EmployeeId), IsUnique = true)]
    public class Contributor
    {
        public int Id {  get; set; }
        public Employee Employee { get; set; } = null!;
        public int EmployeeId { get; set; }
        public ProjectTask ProjectTask { get; set; } = null!;
        public int ProjectTaskId { get; set; }
        public bool IsMandatory { get; set; } 

    }
}
