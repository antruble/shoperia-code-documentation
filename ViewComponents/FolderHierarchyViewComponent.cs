namespace ShoperiaDocumentation.Models
{
    public class FolderHierarchyViewModel
    {
        public string? RootFolderName { get; set; }
        public string? SubFolderName { get; set; }
        public string? RemainingPath { get; set; }
        public IEnumerable<FolderModel>? Folders { get; set; }
        public IEnumerable<FileModel>? Files { get; set; }

        public FolderHierarchyViewModel()
        {
            RootFolderName = null;
            SubFolderName = null;
            RemainingPath = null;
            Folders = null;
            Files = null;
        }
    }
}
