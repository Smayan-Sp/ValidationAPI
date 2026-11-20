using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models.DTO
{
    public class ComputerSystemDTO
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(100)]
        public required string Code { get; set; }
        public required IdNameDTO Department { get; set; }
        public required IdNameDTO Location { get; set; }
        public required IdNameDTO Owner { get; set; }
        public List<ValidationHistoryDTO> ValidationHistory { get; set; } = [];

    }
}
