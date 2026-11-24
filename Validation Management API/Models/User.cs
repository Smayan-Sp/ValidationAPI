using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Validation_Management_API.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(EmployeeId), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public required string Email { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "User";

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}
