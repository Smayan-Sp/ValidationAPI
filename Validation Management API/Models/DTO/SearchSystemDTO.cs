using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class SearchSystemDTO : PagedRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? DepartmentId { get; set; }
        public int? LocationId { get; set; }
        public int? OwnerId { get; set; }
        public ValidationStatus? ValidationStatus { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public SystemSortBy SortBy { get; set; } = SystemSortBy.Name;
        public bool Descending { get; set; }
    }
}
