using System.ComponentModel.DataAnnotations;

namespace Validation_Management_API.Models
{
    public class DocumentList
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        public int Duration { get; set; }
        public int AuthorCount { get; set; }
        public int ReviewerCount { get; set; }
        public int ApproverCount { get; set; }
        public List<DocumentList> DependsOn { get; set; } = [];
    }
}
