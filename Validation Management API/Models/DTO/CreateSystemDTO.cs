using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class CreateSystemDTO
    {
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Code { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int OwnerId { get; set; }
    }
}
