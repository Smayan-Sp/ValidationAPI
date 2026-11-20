using Microsoft.AspNetCore.Mvc;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services.Interfaces
{
    public interface IComputerSystemService
    {
        Task<ServiceResult<ComputerSystemDTO>> GetSystem(int id);
        Task<ServiceResult<int>> CreateSystem(CreateSystemDTO dto);
        Task<ServiceResult<SystemSummaryDTO>> GetSystemSummary(int id);
        Task<ServiceResult<List<SystemSearchResultDTO>>> SearchSystem(SearchSystemDTO dto);
        Task<ServiceResult<bool>> DeactivateSystem(int id);
        Task<ServiceResult<bool>> RestoreSystem(int id);
    }
}
