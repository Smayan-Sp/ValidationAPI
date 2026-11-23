using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class AddPlanDocumentDTO
    {
        [Range(1, int.MaxValue)]
        public int DocumentListId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
    }
}
 