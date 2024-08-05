using Microsoft.AspNetCore.Mvc;
using ShoperiaDocumentation.Models;

namespace ShoperiaDocumentation.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FolderModel>> GetRootFoldersAsync();
        Task<IEnumerable<FolderModel>> GetFoldersAsync(int parentId);
        Task<IEnumerable<FolderModel>> GetFoldersByPathAsync(string path);
        Task<int> GetFolderIdByNameAndParentIdAsync(string name, int? parentId);
        Task<FolderViewModel> GetDataFromUrlAsync(string path);
    }
}
