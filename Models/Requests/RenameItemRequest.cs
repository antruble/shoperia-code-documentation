namespace ShoperiaDocumentation.Models.Requests
{
    public class RenameItemRequest
    {
        public int ItemId { get; set; }
        public string NewName { get; set; }
        public string Type {  get; set; }
    }
}
