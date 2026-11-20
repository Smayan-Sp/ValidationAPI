using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models
{
    [Index(nameof(ProjectDocumentId), nameof(Type), nameof(Number), IsUnique = true)]
    public class ProjectTask
    {
        public int Id { get; set; }

        public int ProjectDocumentId { get; set; }
        public ProjectDocument ProjectDocument { get; set; } = null!;

        public TaskType Type { get; set; }
        public int Number { get; set; } = 1;

        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }

        public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.NotStarted;

        public bool IsOverdue { get; set; }

        public List<Contributor> Contributors { get; set; } = [];
    }
}
