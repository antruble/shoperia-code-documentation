using ShoperiaDocumentation.Models;

namespace ShoperiaDocumentation.Models.ViewModels
{
    public class DatabaseViewModel
    {
        public IEnumerable<FileModel>? Entities { get; set; }

        public DatabaseViewModel()
        {
            Entities = null;
        }
    }
}
