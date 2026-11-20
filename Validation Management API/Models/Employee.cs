using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
        
        public required bool IsActive { get; set; }

    }
}
