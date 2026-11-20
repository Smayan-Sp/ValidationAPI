using Microsoft.AspNetCore.Mvc;
using System.Xml;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Interfaces;

namespace Validation_Management_API.Controllers
{
    [Route("Api/Project")]
    [ApiController]
    public class ProjectController(IProjectService projectService) : ApiBaseController
    {
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectDTO>> GetProject(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            return ToActionResult(await projectService.GetProject(id));
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateProject(CreateProjectDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Please enter valid details");
            }
            var result = await projectService.CreateProject(dto);

            if (result.ResultType == Outcome.CreatedAtAction)
            {
                return CreatedAtAction(nameof(GetProject), new { id = result.Data }, result.Data);
            }

            return ToActionResult(result);
        }

        [HttpGet("Search")]
        public async Task<ActionResult<List<ProjectSummaryDTO>>> SearchProject([FromQuery] SearchProjectDTO dto)
        {
            var result = await projectService.SearchProject(dto);
            return ToActionResult(result);
        }

        [HttpPost("{id:int}/clone")]
        public async Task<ActionResult<int>> CloneProject(int id, CloneProjectDTO dto)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            var result = await projectService.CloneProject(id, dto);

            if (result.ResultType == Outcome.CreatedAtAction)
            {
                return CreatedAtAction(nameof(GetProject), new { id = result.Data }, result.Data);
            }

            return ToActionResult(result);
        }

        [HttpGet("{id:int}/dashboard")]
        public async Task<ActionResult<DashboardDTO>> GetDashboard(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            return ToActionResult(await projectService.GetDashboard(id));
        }
    }
}
