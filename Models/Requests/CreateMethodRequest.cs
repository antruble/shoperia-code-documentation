namespace ShoperiaDocumentation.Models.Requests
{
    public class CreateMethodRequest
    {
        public required string Name { get; set; }
        public required int FileId { get; set; }
        public List<string>? Description { get; set; }
        public required string Code { get; set; }
        public required string Status { get; set; }
    }
}
