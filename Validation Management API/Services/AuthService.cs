using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Validation_Management_API.Data;
using Validation_Management_API.Models;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Interfaces;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<ServiceResult<int>> Register(RegisterRequestDTO dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            Employee? employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (employee == null)
            {
                return ServiceResult<int>.NotFound($"Employee with id {dto.EmployeeId} does not exist");
            }

            if (!employee.IsActive)
            {
                return ServiceResult<int>.BadRequest($"Employee with id {dto.EmployeeId} is inactive");
            }

            bool employeeLinked = await _db.Users.AnyAsync(u => u.EmployeeId == dto.EmployeeId);
            if (employeeLinked)
            {
                return ServiceResult<int>.Conflict("That employee already has an account");
            }

            bool emailTaken = await _db.Users.AnyAsync(u => u.Email == email);
            if (emailTaken)
            {
                return ServiceResult<int>.Conflict("A user with that email already exists");
            }

            User user = new User
            {
                Name = employee.Name,
                Email = email,
                EmployeeId = employee.Id,
                Role = "User"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return ServiceResult<int>.Ok(user.Id);
        }

        public async Task<ServiceResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return ServiceResult<LoginResponseDTO>.BadRequest("Invalid email or password");
            }

            PasswordVerificationResult verification =
                _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return ServiceResult<LoginResponseDTO>.BadRequest("Invalid email or password");
            }

            DateTime expiresAt = DateTime.UtcNow.AddHours(8);

            LoginResponseDTO response = new LoginResponseDTO
            {
                Token = GenerateJwtToken(user, expiresAt),
                ExpiresAt = expiresAt,
                UserId = user.Id,
                EmployeeId = user.EmployeeId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return ServiceResult<LoginResponseDTO>.Ok(response);
        }

        private string GenerateJwtToken(User user, DateTime expiresAt)
        {
            string? secret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            {
                throw new InvalidOperationException("JwtSettings:Secret must be configured and at least 32 characters long");
            }

            byte[] key = Encoding.UTF8.GetBytes(secret);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("employeeId", user.EmployeeId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            JsonWebTokenHandler handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }
    }
}
