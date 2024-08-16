namespace ShoperiaDocumentation.Models.Requests
{
    public class CreateItemRequest
    {
        public required string Name { get; set; }
        public required int ParentId { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
    }
}
