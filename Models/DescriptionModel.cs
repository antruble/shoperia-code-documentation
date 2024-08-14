using System.ComponentModel.DataAnnotations;

namespace ShoperiaDocumentation.Models
{
    public class DescriptionModel
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public int MethodId { get; set; }
        public MethodModel Method { get; set; }
    }

}
