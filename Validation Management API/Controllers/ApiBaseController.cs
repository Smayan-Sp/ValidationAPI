using Microsoft.AspNetCore.Mvc;
using Validation_Management_API.Services.Results;

namespace Validation_Management_API.Controllers
{
    public abstract class ApiBaseController : ControllerBase
    {
        protected int? CurrentEmployeeId
        {
            get
            {
                string? value = User.FindFirst("employeeId")?.Value;
                return int.TryParse(value, out int employeeId) ? employeeId : null;
            }
        }

        protected ActionResult<T> ToActionResult<T>(ServiceResult<T> result)
        {
            return result.ResultType switch
            {
                Outcome.Ok => Ok(result.Data),
                Outcome.NoContent => NoContent(),
                Outcome.NotFound => NotFound(result.Message),
                Outcome.BadRequest => BadRequest(result.Message),
                Outcome.Conflict => Conflict(result.Message),
                Outcome.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Message),
                _ => StatusCode(500, result.Message)
            };
        }
    }
}
