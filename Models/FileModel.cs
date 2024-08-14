using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoperiaDocumentation.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public string Status { get; set; } // "New" or "Modified"

        [ForeignKey("ParentId")]
        public FolderModel ParentFolder { get; set; }

        public IEnumerable<MethodModel> Methods { get; set; }

    }
}
