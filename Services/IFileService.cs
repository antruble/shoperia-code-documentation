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
        #region FOLDER CREATE/RENAME/DELETE
        Task<bool> CreateFolderAsync(string name, int parentId, ClaimsPrincipal user);
        Task<bool> DeleteFolderAsync(int folderId, ClaimsPrincipal user);
        Task<bool> RenameFolderAsync(int folderId, string newFolderName, ClaimsPrincipal user);
        #endregion
        #region FILE CREATE/RENAME/DELETE
        Task<bool> CreateFileAsync(string name, int parentId, ClaimsPrincipal user);
        Task<bool> DeleteFileAsync(int folderId, ClaimsPrincipal user);
        Task<bool> RenameFileAsync(int folderId, string newFolderName, ClaimsPrincipal user);
        #endregion
    }
}
