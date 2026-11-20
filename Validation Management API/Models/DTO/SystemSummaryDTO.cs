using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class SystemSummaryDTO
    {
        public int Id {  get; set; }
        [MaxLength(100)]
        public required String Name {  get; set; }
        public int TotalProjects { get; set; }
        public int CurrentlyActiveProjectCount { get; set; }
        public DateTime? LatestValidationDate { get; set; }

    }
}
