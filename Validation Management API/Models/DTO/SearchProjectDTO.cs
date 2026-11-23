using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class SearchProjectDTO : PagedRequest
    {
        public string? Name { get; set; }
        public ProjectStatus? Status { get; set; }
        public int? SystemId { get; set; }
        public int? LocationId { get; set; }
        public int? ManagerId { get; set; }
        public ProjectValidationType? ValidationType { get; set; }
        public DateTime? PlannedFrom { get; set; }
        public DateTime? PlannedTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public ProjectSortBy SortBy { get; set; } = ProjectSortBy.Name;
        public bool Descending { get; set; }
    }
}
