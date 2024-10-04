namespace ShoperiaDocumentation.Models
{
    public class FileContentViewModel
    {
        public required int FileId { get; set; }
        public required string FileName { get; set; }
        public required bool IsNew { get; set; }
        public bool IsEntity { get; set; } = false;
        public bool IsMapping { get; set; } = false;
        public IEnumerable<MethodModel> Methods { get; set; } = Enumerable.Empty<MethodModel>();
        public IEnumerable<FieldModel> Fields { get; set; } = Enumerable.Empty<FieldModel>();
        public MappingModel? Mapping { get; set; } = null;

    }
}
