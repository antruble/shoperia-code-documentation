using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoperiaDocumentation.Models
{
    public class MethodModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public required string Status { get; set; } // "New" or "Modified"
        public string? Description { get; set; }
        
        [StringLength(int.MaxValue)]
        public string? FullCode { get; set; }

        // Külső kulcs a kapcsolódó fájl azonosítójához
        [ForeignKey("FileModel")]
        public required int FileId { get; set; }

        // Navigációs tulajdonság a kapcsolódó fájlhoz
        public virtual required FileModel FileModel { get; set; }
    }
}
