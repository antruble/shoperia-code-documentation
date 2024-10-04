using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ShoperiaDocumentation.Models;
using System.Security.Claims;
using static ShoperiaDocumentation.Services.FileProcessingService;

namespace ShoperiaDocumentation.Services
{
    public interface IFileService
    {
        Task<IEnumerable<FolderModel>> GetFoldersAsync(int? parentId);
        Task<int> GetFolderIdByNameAndParentIdAsync(string name, int? parentId);
        Task<FolderHierarchyViewModel> GetFolderHierarchyFromPathAsync(string path);
        Task<int?> GetDeepestParentIdAsync(string path);
        Task<FileContentViewModel> GetFileContentAsync(int fileId, bool isEntity = false, bool isMapping = false);
        
        #region FOLDER CREATE/RENAME/DELETE
        Task<int?> CreateFolderAsync(string name, string status, int? parentId, ClaimsPrincipal user);
        Task<bool> DeleteFolderAsync(int folderId, ClaimsPrincipal user);
        Task<bool> RenameFolderAsync(int folderId, string newFolderName, ClaimsPrincipal user);
        #endregion
        
        #region FILE CREATE/RENAME/DELETE
        Task<int?> CreateFileAsync(string name, string status, int parentId, ClaimsPrincipal user, bool isEntity = false, bool isMapping = false);
        Task<bool> DeleteFileAsync(int folderId, ClaimsPrincipal user);
        Task<bool> RenameFileAsync(int folderId, string newFolderName, ClaimsPrincipal user);
        #endregion
        
        #region METHOD
        Task<bool> CreateMethodAsync(int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user);
        Task<bool> CreateOrUpdateMethodAsync(int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user);
        Task<bool> UpdateMethodAsync(int methodId, int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user);
        Task<bool> DeleteMethodAsync(int methodId, ClaimsPrincipal user);
        #endregion
        
        #region FIELD
        Task<bool> CreateFieldAsync(int fileId, FieldData field, ClaimsPrincipal user);
        Task<bool> CreateOrUpdateFieldAsync(int fileId, FieldData field, ClaimsPrincipal user);
        Task<bool> UpdateFieldAsync(int fieldId, int fileId, FieldData field, ClaimsPrincipal user);
        Task<bool> DeleteFieldAsync(int fieldId, ClaimsPrincipal user);

        #endregion

        #region MAPPING
        Task<bool> CreateOrUpdateMappingAsync(MappingData mapping, int parentFileId, ClaimsPrincipal user);
        Task<bool> CreateMappingAsync(MappingData mapping, int parentFileId, ClaimsPrincipal user);
        Task<bool> UpdateMappingAsync(int mappingId, string code, ClaimsPrincipal user);
        Task<bool> DeleteMappingAsync(int mappingId, ClaimsPrincipal user);

        #endregion

        #region SERACHING METHODS
        Task<int?> GetFolderIdByNameAndParentId(string name, int? parentId);
        Task<bool> FileExistsAsync(string filePath);
        Task<FileModel?> GetFileByPathAsync(string filePath);

        #endregion

    }
}
