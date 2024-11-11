namespace ShoperiaDocumentation.Models.Requests
{
    public class UpdateFieldRequest
    {
        public int FieldId { get; set; } // Mező egyedi azonosítója
        public string Name { get; set; } // Új leírás
        public string Type { get; set; } // Új leírás
        public string Description { get; set; } // Új leírás
        public int FileId { get; set; } // Új leírás
    }
}
