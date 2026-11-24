using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Services.Interfaces
{
    public interface IComputerSystemService
    {
        Task<ServiceResult<ComputerSystemDTO>> GetSystem(int id);
        Task<ServiceResult<int>> CreateSystem(CreateSystemDTO dto);
        Task<ServiceResult<SystemSummaryDTO>> GetSystemSummary(int id);
        Task<ServiceResult<PagedResult<SystemSearchResultDTO>>> SearchSystem(SearchSystemDTO dto);
        Task<ServiceResult<bool>> DeactivateSystem(int id);
        Task<ServiceResult<bool>> RestoreSystem(int id);
        Task<ServiceResult<int>> UploadAttachment(int id, IFormFile file);
        Task<ServiceResult<List<AttachmentDTO>>> ListAttachments(int id);
        Task<ServiceResult<AttachmentFileDTO>> DownloadAttachment(int id, int attachmentId);
        Task<ServiceResult<bool>> DeleteAttachment(int id, int attachmentId);
        Task<ServiceResult<bool>> UpdateSystem(int id, UpdateSystemDTO dto);
    }
}
