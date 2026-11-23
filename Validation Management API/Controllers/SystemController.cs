using Microsoft.AspNetCore.Mvc;
using Validation_Management_API.Models.DTO;
using Validation_Management_API.Services.Interfaces;

namespace Validation_Management_API.Controllers
{

    [Route("Api/System")]
    [ApiController]
    public class SystemController(IComputerSystemService computerSystemService) : ApiBaseController
    {

        [HttpGet("{id:int}/system")]
        public async Task<ActionResult<ComputerSystemDTO>> GetSystemWithId (int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            return ToActionResult(await computerSystemService.GetSystem(id));
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateSystem(CreateSystemDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Please enter all System details");
            }
            var result = await computerSystemService.CreateSystem(dto);

            if (result.ResultType == Outcome.CreatedAtAction)
            {
                return CreatedAtAction(nameof(GetSystemWithId), new { id = result.Data }, result.Data);
            }

            return ToActionResult(result);
        }

        [HttpGet("{id:int}/summary")]
        public async Task<ActionResult<SystemSummaryDTO>> GetSystemSummary(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }
            return ToActionResult(await computerSystemService.GetSystemSummary(id));
        }

        [HttpGet("Search")]
        public async Task<ActionResult<PagedResult<SystemSearchResultDTO>>> SearchSystem([FromQuery] SearchSystemDTO dto)
        {
            return ToActionResult(await computerSystemService.SearchSystem(dto));
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<ActionResult<bool>> DeactivateSystem(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }
            return ToActionResult(await computerSystemService.DeactivateSystem(id));
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<bool>> UpdateSystem(int id, UpdateSystemDTO dto)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }
            return ToActionResult(await computerSystemService.UpdateSystem(id, dto));
        }

        [HttpPost("{id:int}/attachments")]
        public async Task<ActionResult<int>> UploadAttachment(int id, IFormFile file)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }
            return ToActionResult(await computerSystemService.UploadAttachment(id, file));
        }

        [HttpGet("{id:int}/attachments")]
        public async Task<ActionResult<List<AttachmentDTO>>> ListAttachments(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }
            return ToActionResult(await computerSystemService.ListAttachments(id));
        }

        [HttpGet("{id:int}/attachments/{attachmentId:int}")]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            if (id <= 0 || attachmentId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }

            var result = await computerSystemService.DownloadAttachment(id, attachmentId);

            if (result.ResultType == Outcome.Ok)
            {
                return File(result.Data!.Content, result.Data.ContentType, result.Data.OriginalName);
            }

            return ToActionResult(result).Result!;
        }

        [HttpDelete("{id:int}/attachments/{attachmentId:int}")]
        public async Task<ActionResult<bool>> DeleteAttachment(int id, int attachmentId)
        {
            if (id <= 0 || attachmentId <= 0)
            {
                return BadRequest("Ids must be greater than 0");
            }
            return ToActionResult(await computerSystemService.DeleteAttachment(id, attachmentId));
        }

        [HttpPatch("{id:int}/restore")]
        public async Task<ActionResult<bool>> RestoreSystem(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }
            return ToActionResult(await computerSystemService.RestoreSystem(id));
        }

    }


}
