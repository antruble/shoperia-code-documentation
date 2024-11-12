using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoperiaDocumentation.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public string Status { get; set; } // "New" or "Modified"

        [ForeignKey("ParentId")]
        public FolderModel? ParentFolder { get; set; }
        public int? ParentId { get; set; }

        public bool IsEntity { get; set; } = false;
        public bool IsDatabaseEntity { get; set; } = false;
        public bool IsMapping { get; set; } = false;


        public virtual ICollection<MethodModel> Methods { get; set; } = new List<MethodModel>();

        // IF it's an entity
        public virtual ICollection<FieldModel> Fields { get; set; } = new List<FieldModel>();

        // If it's a mapping
        public MappingModel? Mapping { get; set; } = null;
    }
    public class FieldModel
    {
        [Key]
        public int Id { get; set; }

        // Külső kulcs a kapcsolódó fájl azonosítójához
        [ForeignKey("FileModel")]
        public required int FileId { get; set; }
        // Navigációs tulajdonság a kapcsolódó fájlhoz
        //public virtual required FileModel FileModel { get; set; }

        [Required]
        [StringLength(255)]
        public required string Name { get; set; }

        [Required]
        [StringLength(255)]
        public required string Type { get; set; }

        [Required]
        [StringLength(255)]
        public required string Description { get; set; } = string.Empty;

        public bool IsNullable { get; set; } = false;      // Null megengedett
        public string? DefaultValue { get; set; }          // Alapértelmezett érték
        public string? Comment { get; set; }               // Oszlophoz fűzött komment

        public bool IsPrimaryKey { get; set; } = false;
        public bool IsForeignKey { get; set; } = false;
        public string? ForeignTable { get; set; }
    }
    public class MappingModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string RelativePath { get; set; }

        [Required]
        public string Code { get; set; }

        public required bool IsNew { get; set; }
        public required string ParentEntitysName { get; set; }

        [ForeignKey("ParentId")]
        public FileModel? ParentFile { get; set; }
        public int? ParentId { get; set; }
    }
}
