using Validation_Management_API.Models.DTO;
using Validation_Management_API.Models.Enums;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ServiceResult<ProjectDTO>> GetProject(int id);
        Task<ServiceResult<int>> CreateProject(CreateProjectDTO dto);
        Task<ServiceResult<List<ProjectSummaryDTO>>>SearchProject(SearchProjectDTO dto);
        Task<ServiceResult<DashboardDTO>> GetDashboard(int id);
        Task<ServiceResult<int>> CloneProject(int sourceProjectId, CloneProjectDTO dto);
        Task<ServiceResult<bool>> ChangeStatus(int id, ProjectStatus status);
    }
}
