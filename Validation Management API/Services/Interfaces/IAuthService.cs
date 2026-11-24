using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<int>> Register(RegisterRequestDTO dto);
        Task<ServiceResult<LoginResponseDTO>> Login(LoginRequestDTO dto);
    }
}
