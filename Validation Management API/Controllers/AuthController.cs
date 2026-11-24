using Microsoft.AspNetCore.Mvc;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Interfaces;

namespace Validation_Management_API.Controllers
{
    [Route("Api/Auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ApiBaseController
    {
        [HttpPost("register")]
        public async Task<ActionResult<int>> Register(RegisterRequestDTO dto)
        {
            return ToActionResult(await authService.Register(dto));
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
        {
            return ToActionResult(await authService.Login(dto));
        }
    }
}
