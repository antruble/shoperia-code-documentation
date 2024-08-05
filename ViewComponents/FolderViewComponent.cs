namespace ShoperiaDocumentation.Models
{
    public class FolderViewModel
    {
        public string? RootName { get; set; }
        public string? SubRootName { get; set; }
        public string? RelativePath { get; set; }
        public IEnumerable<FolderModel>? Data { get; set; }
        public FolderViewModel() 
        { 
            RootName = null;
            SubRootName = null;
            RelativePath = null;
            Data = null;
        }
    }
}
