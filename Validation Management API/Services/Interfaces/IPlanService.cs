using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services.Interfaces
{
    public interface IPlanService
    {
        Task<ServiceResult<PlanDTO>> GetPlan(int projectId);
        Task<ServiceResult<AddPlanDocumentsResultDTO>> AddPlanDocuments(int projectId, List<AddPlanDocumentDTO> documents);
        Task<ServiceResult<bool>> RemovePlanDocuments(int projectId, List<int> planDocumentIds);
        Task<ServiceResult<bool>> UpdateTask(int projectId, int taskId, UpdateTaskDTO dto);
        Task<ServiceResult<bool>> AddContributor(int projectId, int taskId, TaskContributorInputDTO dto);
        Task<ServiceResult<bool>> ReplaceContributor(int projectId, int taskId, int employeeId, TaskContributorInputDTO dto);
        Task<ServiceResult<bool>> RemoveContributor(int projectId, int taskId, int employeeId);
        Task<ServiceResult<bool>> RemoveTask(int projectId, int taskId);
    }
}
