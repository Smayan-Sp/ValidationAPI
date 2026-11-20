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
    }


}
