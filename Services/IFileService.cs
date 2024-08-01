using ShoperiaDocumentation.Models;

namespace ShoperiaDocumentation.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FolderModel>> GetRootFoldersAsync();
        Task<IEnumerable<FolderModel>> GetFoldersAsync(int parentId);
        Task<IEnumerable<FolderModel>> GetMainCategoriesAsync();
    }
}
