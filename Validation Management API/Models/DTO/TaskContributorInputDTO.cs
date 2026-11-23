using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class TaskContributorInputDTO
    {
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }
        public bool IsMandatory { get; set; }
    }
}
 