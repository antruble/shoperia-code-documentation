using System.ComponentModel.DataAnnotations;

namespace ShoperiaDocumentation.Models
{
    public class MethodModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Status { get; set; } // "New" or "Modified"
        public ICollection<DescriptionModel> Descriptions { get; set; }
        public string FullCode { get; set; }
    }
}
