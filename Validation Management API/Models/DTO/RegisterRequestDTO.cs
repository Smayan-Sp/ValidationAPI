using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class RegisterRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public required string Password { get; set; }
    }
}
