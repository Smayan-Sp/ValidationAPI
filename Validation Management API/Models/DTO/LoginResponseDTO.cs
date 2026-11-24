namespace Validation_Management_API.Models.DTO
{
    public class LoginResponseDTO
    {
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
