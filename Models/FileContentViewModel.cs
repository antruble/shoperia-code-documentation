namespace ShoperiaDocumentation.Models
{
    public class FileContentViewModel
    {
        public string FileName { get; set; }
        public IEnumerable<MethodModel> Methods { get; set; }
    }
}
