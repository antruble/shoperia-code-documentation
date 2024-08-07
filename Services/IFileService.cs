using Microsoft.AspNetCore.Mvc;
using ShoperiaDocumentation.Models;
using System.Security.Claims;

namespace ShoperiaDocumentation.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FolderModel>> GetFoldersAsync(int? parentId);
        Task<int> GetFolderIdByNameAndParentIdAsync(string name, int? parentId);
        Task<FolderHierarchyViewModel> GetFolderHierarchyFromPathAsync(string path);
        Task<int?> GetDeepestParentIdAsync(string path);
        Task<FileContentViewModel> GetFileContentAsync(int fileId);
        Task<bool> DeleteFolderAsync(int folderId, ClaimsPrincipal user);
    }
}
