namespace Validation_Management_API.Models.DTO
{
    public class DashboardDTO
    {
        public int ProjectId { get; set; }
        public required string ProjectName { get; set; }
        public int DocumentCount { get; set; }
        public TaskCountsDTO TaskCounts { get; set; } = new();
        public int OverdueTaskCount { get; set; }
        public decimal PercentComplete { get; set; }
    }
}
