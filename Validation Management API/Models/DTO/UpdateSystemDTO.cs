using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class UpdateSystemDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public int? DepartmentId { get; set; }
        public int? OwnerId { get; set; }
    }
}
