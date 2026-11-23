namespace Validation_Management_API.Models.DTO
{
    public class AddPlanDocumentsResultDTO
    {
        public List<IdNameDTO> Added { get; set; } = [];
        public List<IdNameDTO> Skipped { get; set; } = [];
    }
}
