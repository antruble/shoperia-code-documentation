namespace ShoperiaDocumentation.Models.Requests
{
    public class UpdateEntityRequest
    {
        public int Id { get; set; } // Entity ID
        public string Description { get; set; } = string.Empty;
        public List<UpdateEntityFieldRequest> Fields { get; set; } = new();
    }

    public class UpdateEntityFieldRequest
    {
        public string Name { get; set; } = string.Empty; // Field name
        public string Description { get; set; } = string.Empty; // Updated description
    }

}
