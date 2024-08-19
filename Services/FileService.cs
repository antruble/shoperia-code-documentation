using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using System.Security.Claims;

namespace ShoperiaDocumentation.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FileService> _logger;

        public FileService(ApplicationDbContext context, ILogger<FileService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<FolderModel>> GetFoldersAsync(int? parentId)
        {
            try
            {
                return await _context.Folders
                    .Where(f => f.ParentId == parentId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folders for parent ID {ParentId}", parentId);
                throw;
            }
        }
        public async Task<FolderHierarchyViewModel> GetFolderHierarchyFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new FolderHierarchyViewModel();
            }

            var pathSegments = path.Split('/');
            int? parentId = await GetDeepestParentIdAsync(path);

            var rootFolderName = pathSegments.Length > 0 ? pathSegments[0] : string.Empty;
            var subFolderName = pathSegments.Length > 1 ? pathSegments[1] : string.Empty;
            var remainingPath = pathSegments.Length > 2 ? string.Join("/", pathSegments.Skip(2)) : string.Empty;
            var folders = pathSegments.Length > 1 ? await _context.Folders.Where(f => f.ParentId == parentId).ToListAsync() : null;
            var files = pathSegments.Length > 1 ? await _context.Files.Where(f => f.ParentId == parentId).ToListAsync() : null;

            var result = new FolderHierarchyViewModel
            {
                CurrentFolderId = parentId,
                RootFolderName = rootFolderName,
                SubFolderName = subFolderName,
                RemainingPath = remainingPath,
                Folders = folders,
                Files = files
            };

            return result;
        }
        public async Task<int> GetFolderIdByNameAndParentIdAsync(string name, int? parentId)
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Name == name && f.ParentId == parentId);
            if (folder == null)
            {
                return -1; // Vagy használhatsz Nullable<int> típust és null-t adhatsz vissza
            }
            return folder.Id;
        }
        public async Task<int?> GetDeepestParentIdAsync(string path)
        {
            int? parentId = null;
            var pathSegments = path.Split('/');

            foreach (var segment in pathSegments)
            {
                parentId = await GetFolderIdByNameAndParentIdAsync(segment, parentId);
            }

            return parentId;
        }
        public async Task<FileContentViewModel> GetFileContentAsync(int fileId)
        {
            // Példa implementáció
            var file = await _context.Files.Include(f => f.Methods).FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
                // Handle file not found
                return null;
            }

            var fileContent = new FileContentViewModel
            {
                FileId = fileId,
                FileName = file.Name,
                Methods = file.Methods
            };

            return fileContent;
        }

        #region FOLDER CREATE/RENAME/DELETE
        public async Task<bool> CreateFolderAsync(string folderName, string status, int parentId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create folder {FolderId} without admin permissions.", user.Identity?.Name, folderName);
                return false;
            }

            if (string.IsNullOrEmpty(folderName) || string.IsNullOrWhiteSpace(folderName))
            {
                _logger.LogWarning("Invalid folder name {FolderName} provided for creation by user {UserName}.", folderName, user.Identity?.Name);
                return false;
            }

            bool nameAlreadyExist = await _context.Folders.AnyAsync(f => f.ParentId == parentId && f.Name == folderName);
            if (nameAlreadyExist)
            {
                _logger.LogError($"Failed to create {folderName} folder, because there is already a folder with this name in its directory.");
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newFolder = new FolderModel
                {
                    Name = folderName,
                    ParentId = parentId,
                    Status = status,
                };

                _context.Folders.Add(newFolder);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Folder {FolderName} successfully created by user {UserName}.", folderName, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create folder {FolderName} by user {UserName}.", folderName, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the folder {FolderName} by user {UserName}.", folderName, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> DeleteFolderAsync(int folderId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to delete folder {FolderId} without admin permissions.", user.Identity?.Name, folderId);
                return false;
            }

            if (folderId <= 0)
            {
                _logger.LogWarning("Invalid folder ID {FolderId} provided for deletion by user {UserName}.", folderId, user.Identity?.Name);
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var folder = await _context.Folders.FindAsync(folderId);
                    if (folder == null)
                    {
                        _logger.LogWarning("Folder with ID {FolderId} not found for deletion by user {UserName}.", folderId, user.Identity?.Name);
                        return false;
                    }

                    _context.Folders.Remove(folder);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("Folder with ID {FolderId} successfully deleted by user {UserName}.", folderId, user.Identity?.Name);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("No changes detected while attempting to delete folder with ID {FolderId} by user {UserName}.", folderId, user.Identity?.Name);
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting folder with ID {FolderId} by user {UserName}.", folderId, user.Identity?.Name);
                    return false;
                }
            }
        }
        public async Task<bool> RenameFolderAsync(int folderId, string newFolderName, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to rename folder {FolderId} without admin permissions.", user.Identity?.Name, folderId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(newFolderName))
            {
                _logger.LogWarning("New folder name is invalid for folder ID {FolderId} by user {UserName}.", folderId, user.Identity?.Name);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var folder = await _context.Folders.FindAsync(folderId);
                if (folder == null)
                {
                    _logger.LogWarning("Folder with ID {FolderId} not found for renaming by user {UserName}.", folderId, user.Identity?.Name);
                    return false;
                }
                var folderWithTheNewName = await _context.Folders
                    .FirstOrDefaultAsync(f => f.ParentId == folder.ParentId && f.Name == newFolderName);
                if (folderWithTheNewName != null)
                {
                    _logger.LogError("Folder with the new name '{NewFolderName}' already exists in the same directory for user {UserName}.", newFolderName, user.Identity?.Name);
                    return false;
                }
                folder.Name = newFolderName;
                _context.Folders.Update(folder);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Folder with ID {FolderId} renamed to {NewFolderName} by user {UserName}.", folderId, newFolderName, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to rename folder with ID {FolderId} by user {UserName}.", folderId, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while renaming the folder with ID {FolderId} by user {UserName}.", folderId, user.Identity?.Name);
                return false;
            }
        }

        #endregion
        #region FILE CREATE/RENAME/DELETE
        public async Task<bool> CreateFileAsync(string name, string status, int parentId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create file {FileName} without admin permissions.", user.Identity?.Name, name);
                return false;
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Invalid file name {FileName} provided for creation by user {UserName}.", name, user.Identity?.Name);
                return false;
            }

            bool nameAlreadyExist = await _context.Files.AnyAsync(f => f.ParentId == parentId && f.Name == name);
            if (nameAlreadyExist)
            {
                _logger.LogError("Failed to create {FileName} file, because there is already a file with this name in its directory.", name);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newFile = new FileModel
                {
                    Name = name,
                    ParentId = parentId,
                    Status = status,
                };

                _context.Files.Add(newFile);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("File {FileName} successfully created by user {UserName}.", name, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create file {FileName} by user {UserName}.", name, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the file {FileName} by user {UserName}.", name, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> DeleteFileAsync(int fileId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to delete file {FileId} without admin permissions.", user.Identity?.Name, fileId);
                return false;
            }

            if (fileId <= 0)
            {
                _logger.LogWarning("Invalid file ID {FileId} provided for deletion by user {UserName}.", fileId, user.Identity?.Name);
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var file = await _context.Files.FindAsync(fileId);
                    if (file == null)
                    {
                        _logger.LogWarning("File with ID {FileId} not found for deletion by user {UserName}.", fileId, user.Identity?.Name);
                        return false;
                    }

                    _context.Files.Remove(file);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("File with ID {FileId} successfully deleted by user {UserName}.", fileId, user.Identity?.Name);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("No changes detected while attempting to delete file with ID {FileId} by user {UserName}.", fileId, user.Identity?.Name);
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting file with ID {FileId} by user {UserName}.", fileId, user.Identity?.Name);
                    return false;
                }
            }
        }
        public async Task<bool> RenameFileAsync(int fileId, string newFileName, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to rename file {FileId} without admin permissions.", user.Identity?.Name, fileId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(newFileName))
            {
                _logger.LogWarning("New file name is invalid for file ID {FileId} by user {UserName}.", fileId, user.Identity?.Name);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var file = await _context.Files.FindAsync(fileId);
                if (file == null)
                {
                    _logger.LogWarning("File with ID {FileId} not found for renaming by user {UserName}.", fileId, user.Identity?.Name);
                    return false;
                }

                var fileWithTheNewName = await _context.Files
                    .FirstOrDefaultAsync(f => f.ParentId == file.ParentId && f.Name == newFileName);
                if (fileWithTheNewName != null)
                {
                    _logger.LogError("File with the new name '{NewFileName}' already exists in the same directory for user {UserName}.", newFileName, user.Identity?.Name);
                    return false;
                }

                file.Name = newFileName;
                _context.Files.Update(file);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("File with ID {FileId} renamed to {NewFileName} by user {UserName}.", fileId, newFileName, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to rename file with ID {FileId} by user {UserName}.", fileId, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while renaming the file with ID {FileId} by user {UserName}.", fileId, user.Identity?.Name);
                return false;
            }
        }

        #endregion
        #region METHOD CREATE
        public async Task<bool> CreateMethodAsync(int fileId, string methodName, List<string>? descriptions, string methodCode, string methodStatus, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create method {MethodName} without admin permissions.", user.Identity?.Name, methodName);
                return false;
            }

            if (string.IsNullOrEmpty(methodName) || string.IsNullOrWhiteSpace(methodName))
            {
                _logger.LogWarning("Invalid method name {MethodName} provided for creation by user {UserName}."
                    , methodName, user.Identity?.Name);
                return false;
            }

            bool nameAlreadyExist = await _context.Methods.AnyAsync(f => f.FileId == fileId && f.Name == methodName);
            if (nameAlreadyExist)
            {
                _logger.LogError($"Failed to create {methodName} method, because there is already a method with this name in the same file.");
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newMethod = new MethodModel
                {
                    Name = methodName,
                    FileId = fileId,
                    Descriptions = descriptions?.Select(desc => new DescriptionModel { Content = desc }).ToList(),
                    FullCode = methodCode,
                    Status = methodStatus,
                };

                _context.Methods.Add(newMethod);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Method {MethodName} successfully created by user {UserName}.", methodName, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create method {MethodName} by user {UserName}.", methodName, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the method {Method} by user {UserName}.", methodName, user.Identity?.Name);
                return false;
            }
        }
        #endregion

    }
}
