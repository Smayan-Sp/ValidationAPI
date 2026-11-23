using Microsoft.AspNetCore.Mvc;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Interfaces;

namespace Validation_Management_API.Controllers
{
    [Route("Api/Project/{projectId:int}/plan")]
    [ApiController]
    public class PlanController(IPlanService planService) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PlanDTO>> GetPlan(int projectId)
        {
            if (projectId <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            return ToActionResult(await planService.GetPlan(projectId));
        }

        [HttpPost("documents")]
        public async Task<ActionResult<AddPlanDocumentsResultDTO>> AddDocuments(int projectId, List<AddPlanDocumentDTO> documents)
        {
            if (projectId <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            return ToActionResult(await planService.AddPlanDocuments(projectId, documents));
        }

        [HttpDelete("documents")]
        public async Task<ActionResult<bool>> RemoveDocuments(int projectId, [FromQuery] List<int> documentIds)
        {
            if (projectId <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            return ToActionResult(await planService.RemovePlanDocuments(projectId, documentIds));
        }

        [HttpPatch("tasks/{taskId:int}")]
        public async Task<ActionResult<bool>> UpdateTask(int projectId, int taskId, UpdateTaskDTO dto)
        {
            if (projectId <= 0 || taskId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }

            return ToActionResult(await planService.UpdateTask(projectId, taskId, dto));
        }

        [HttpPost("tasks/{taskId:int}/contributors")]
        public async Task<ActionResult<bool>> AddContributor(int projectId, int taskId, TaskContributorInputDTO dto)
        {
            if (projectId <= 0 || taskId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }

            return ToActionResult(await planService.AddContributor(projectId, taskId, dto));
        }

        [HttpPut("tasks/{taskId:int}/contributors/{employeeId:int}")]
        public async Task<ActionResult<bool>> ReplaceContributor(int projectId, int taskId, int employeeId, TaskContributorInputDTO dto)
        {
            if (projectId <= 0 || taskId <= 0 || employeeId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }

            return ToActionResult(await planService.ReplaceContributor(projectId, taskId, employeeId, dto));
        }

        [HttpDelete("tasks/{taskId:int}/contributors/{employeeId:int}")]
        public async Task<ActionResult<bool>> RemoveContributor(int projectId, int taskId, int employeeId)
        {
            if (projectId <= 0 || taskId <= 0 || employeeId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }

            return ToActionResult(await planService.RemoveContributor(projectId, taskId, employeeId));
        }

        [HttpDelete("tasks/{taskId:int}")]
        public async Task<ActionResult<bool>> RemoveTask(int projectId, int taskId)
        {
            if (projectId <= 0 || taskId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }

            return ToActionResult(await planService.RemoveTask(projectId, taskId));
        }
    }


}
