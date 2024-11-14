using System.ComponentModel.DataAnnotations;

namespace ShoperiaDocumentation.Models
{
    public class ExportFileDto
    {
        public string Path { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<ExportMethodDto> Methods { get; set; } = new();
    }

    public class ExportMethodDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
    }

    public class ExportEntityDto
    {
        public string Path { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public bool IsDatabaseEntity { get; set; }
        public List<ExportFieldDto> Fields { get; set; } = new();
    }

    public class ExportFieldDto
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }

        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public string? ForeignTable { get; set; }
    }
}
