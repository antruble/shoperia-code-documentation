using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
        Task<int?> CreateFolderAsync(string name, string status, int? parentId, ClaimsPrincipal user);
        Task<bool> DeleteFolderAsync(int folderId, ClaimsPrincipal user);
        Task<bool> RenameFolderAsync(int folderId, string newFolderName, ClaimsPrincipal user);
        #endregion
        #region FILE CREATE/RENAME/DELETE
        Task<int?> CreateFileAsync(string name, string status, int parentId, ClaimsPrincipal user);
        Task<bool> DeleteFileAsync(int folderId, ClaimsPrincipal user);
        Task<bool> RenameFileAsync(int folderId, string newFolderName, ClaimsPrincipal user);
        #endregion
        #region METHOD
        Task<bool> CreateMethodAsync(int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user);
        Task<bool> CreateOrUpdateMethodAsync(int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user);
        Task<bool> UpdateMethodAsync(int methodId, int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user);
        Task<bool> DeleteMethodAsync(int methodId, ClaimsPrincipal user);
        #endregion
        #region SERACHING METHODS
        Task<int?> GetFolderIdByNameAndParentId(string name, int? parentId);
        #endregion
    }
}
