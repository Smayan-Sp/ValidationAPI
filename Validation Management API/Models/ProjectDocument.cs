using Microsoft.EntityFrameworkCore;

namespace Validation_Management_API.Models
{
    [Index(nameof(ProjectId), nameof(DocumentListId), IsUnique = true)]
    public class ProjectDocument
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int DocumentListId { get; set; }
        public DocumentList DocumentList { get; set; } = null!;

        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }

        public List<ProjectTask> Tasks { get; set; } = [];

        public List<ProjectDocument> DependsOn { get; set; } = [];
    }
}
