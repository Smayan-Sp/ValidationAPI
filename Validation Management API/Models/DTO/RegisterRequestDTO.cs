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
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
