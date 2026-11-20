using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services.Interfaces
{
    public interface IPlanService
    {
        Task<ServiceResult<PlanDTO>> GetPlan(int projectId);
    }
}
