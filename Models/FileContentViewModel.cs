namespace ShoperiaDocumentation.Models
{
    public class FileContentViewModel
    {
        public required int FileId { get; set; }
        public required string FileName { get; set; }
        public IEnumerable<MethodModel> Methods { get; set; } = Enumerable.Empty<MethodModel>();
    }
}
