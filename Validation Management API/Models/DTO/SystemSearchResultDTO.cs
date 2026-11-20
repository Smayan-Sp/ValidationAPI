using System.ComponentModel.DataAnnotations;
using Validation_Management_API.Models.Enums;

namespace Validation_Management_API.Models.DTO
{
    public class SystemSearchResultDTO
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Code { get; set; }
        public required string DepartmentName { get; set; }
        public required string LocationName { get; set; }
        public required string OwnerName { get; set; }
        public ValidationStatus ValidationStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
