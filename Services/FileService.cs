using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using ShoperiaDocumentation.Models.ViewModels;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using static ShoperiaDocumentation.Services.FileProcessingService;
using static System.Net.WebRequestMethods;
using static System.Reflection.Metadata.Ecma335.MethodBodyStreamEncoder;

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


            var pathSegments = path.Split('/');
            int? parentId = await GetDeepestParentIdAsync(path);

            var rootFolderName = pathSegments.Length > 0 ? pathSegments[0] : string.Empty;
            var subFolderName = pathSegments.Length > 1 ? pathSegments[1] : string.Empty;
            var remainingPath = pathSegments.Length > 2 ? string.Join("/", pathSegments.Skip(2)) : string.Empty;

            var folders = await _context.Folders
                        .Where(f => f.ParentId == parentId)
                        .OrderBy(f => f.Name)
                        .ToListAsync();

            var files = pathSegments.Length > 1 
                ? await _context.Files
                    .Where(f => f.ParentId == parentId)
                    .Include(f => f.Methods)
                    .OrderBy(f => f.Name)
                    .ToListAsync() 
                : null;

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
                return -1; 
            }
            return folder.Id;
        }
        public async Task<int?> GetDeepestParentIdAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                return null;
            int? parentId = null;
            var pathSegments = path.Split('/');

            foreach (var segment in pathSegments)
            {
                parentId = await GetFolderIdByNameAndParentIdAsync(segment, parentId);
            }

            return parentId;
        }
        public async Task<FileContentViewModel> GetFileContentAsync(int fileId, bool isEntity = false, bool isMapping = false, bool isDatabaseEntity = false)
        {
            FileModel? file;
            FileContentViewModel? responseModel;
            var relativePath = await GetFilePathAsync(fileId);
            if (isEntity)
            {
                file = await _context.Files.Include(f => f.Fields).FirstOrDefaultAsync(f => f.Id == fileId);
                if (file == null)
                    return null; //TODO: handle

                responseModel = new FileContentViewModel
                {
                    FileId = fileId,
                    FileName = file.Name,
                    RelativePath = relativePath,
                    Fields = file.Fields,
                    IsNew = file.Status == "new",
                    IsEntity = true,
                    IsDatabaseEntity = isDatabaseEntity,
                };
            }
            else if (isMapping)
            {
                file = await _context.Files.Include(f => f.Mapping).FirstOrDefaultAsync(f => f.Id == fileId);
                if (file == null)
                    return null; //TODO: handle
                responseModel = new FileContentViewModel
                {
                    FileId = fileId,
                    FileName = file.Name,
                    RelativePath = relativePath,
                    Mapping = file.Mapping,
                    IsNew = file.Status == "new",
                    IsMapping = true
                };
            }
            else
            { 
                file = await _context.Files.Include(f => f.Methods).FirstOrDefaultAsync(f => f.Id == fileId);
               
                if (file == null)
                    return null;

                responseModel = new FileContentViewModel
                {
                    FileId = fileId,
                    FileName = file.Name,
                    RelativePath = relativePath,
                    Methods = file.Methods,
                    IsNew = file.Status == "new"
                };
            }
            
            return responseModel;
        }

        #region FOLDER CREATE/RENAME/DELETE/API
        public async Task<int?> CreateFolderAsync(string folderName, string status, int? parentId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create folder {FolderId} without admin permissions.", user.Identity?.Name, folderName);
                return null;
            }
            if (parentId == null)
            {
                _logger.LogWarning($"User {user.Identity?.Name} attempted to create a root directory with the name of {folderName} (the parentId was null)");
                return null;
            }
            if (string.IsNullOrEmpty(folderName) || string.IsNullOrWhiteSpace(folderName))
            {
                _logger.LogWarning("Invalid folder name {FolderName} provided for creation by user {UserName}.", folderName, user.Identity?.Name);
                return null;
            }

            bool nameAlreadyExist = await _context.Folders.AnyAsync(f => f.ParentId == parentId && f.Name == folderName);
            if (nameAlreadyExist)
            {
                var folder = await _context.Folders.FirstOrDefaultAsync(f => f.ParentId == parentId && f.Name == folderName);
                _logger.LogWarning($"Failed to create {folderName} folder, because there is already a folder with this name in its directory.");
                return folder?.Id;
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
                    return newFolder.Id;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create folder {FolderName} by user {UserName}.", folderName, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the folder {FolderName} by user {UserName}.", folderName, user.Identity?.Name);
                return null;
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
        public async Task<int?> CreateFileAsync(string name, string status, int parentId, ClaimsPrincipal user, bool isEntity = false, bool isMapping = false, bool isDatabaseEntity = false)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create file {FileName} without admin permissions.", user.Identity?.Name, name);
                return null;
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Invalid file name {FileName} provided for creation by user {UserName}.", name, user.Identity?.Name);
                return null;
            }

            bool nameAlreadyExist = await _context.Files.AnyAsync(f => f.ParentId == parentId && f.Name == name);
            if (nameAlreadyExist)
            {
                var file = await _context.Files.FirstOrDefaultAsync(f => f.Name == name && f.ParentId == parentId);
                if (isEntity)
                {
                    if (!file.IsEntity)
                        file.IsEntity = true;
                    if (file.IsDatabaseEntity != isDatabaseEntity)
                        file.IsDatabaseEntity = isDatabaseEntity;
                    
                    try
                    {
                        _context.Files.Update(file);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to update {FileName} file while tried to update isEntity or isDatabaseEntity: {ex}", name, ex.Message);
                    }
                }
                _logger.LogWarning("Failed to create {FileName} file, because there is already a file with this name in its directory.", name);
                return file?.Id;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newFile = new FileModel
                {
                    Name = name,
                    ParentId = parentId,
                    Status = status,
                    IsEntity = isEntity,
                    IsDatabaseEntity = isDatabaseEntity,
                    IsMapping = isMapping,
                };

                _context.Files.Add(newFile);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("File {FileName} successfully created by user {UserName}.", name, user.Identity?.Name);
                    return newFile.Id;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create file {FileName} by user {UserName}.", name, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the file {FileName} by user {UserName}.", name, user.Identity?.Name);
                return null;
            }
        }

        public async Task<int?> CreateFileAsync(FileModel file, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create file {FileName} without admin permissions.", user.Identity?.Name, file.Name);
                return null;
            }

            if (string.IsNullOrEmpty(file.Name) || string.IsNullOrWhiteSpace(file.Name))
            {
                _logger.LogWarning("Invalid file name {FileName} provided for creation by user {UserName}.", file.Name, user.Identity?.Name);
                return null;
            }

            bool nameAlreadyExist = await _context.Files.AnyAsync(f => f.ParentId == file.ParentId && f.Name == file.Name);
            if (nameAlreadyExist)
            {
                var existingFile = await _context.Files.FirstOrDefaultAsync(f => f.Name == file.Name && f.ParentId == file.ParentId);
                if (file.IsEntity)
                {
                    if (!existingFile.IsEntity)
                        existingFile.IsEntity = true;
                    if (existingFile.IsDatabaseEntity != file.IsDatabaseEntity)
                        existingFile.IsDatabaseEntity = file.IsDatabaseEntity;

                    try
                    {
                        _context.Files.Update(existingFile);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to update {FileName} file while tried to update isEntity or isDatabaseEntity: {ex}", file.Name, ex.Message);
                    }
                }
                _logger.LogWarning("Failed to create {FileName} file, because there is already a file with this name in its directory.", file.Name);
                return existingFile?.Id;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newFile = new FileModel
                {
                    Name = file.Name,
                    Description = file.Description,
                    ParentId = file.ParentId,
                    Status = file.Status,
                    IsEntity = file.IsEntity,
                    IsDatabaseEntity = file.IsDatabaseEntity,
                    IsMapping = file.IsMapping,


                };

                _context.Files.Add(newFile);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("File {FileName} successfully created by user {UserName}.", file.Name, user.Identity?.Name);
                    return newFile.Id;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create file {FileName} by user {UserName}.", file.Name, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return null;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the file {FileName} by user {UserName}.", file.Name, user.Identity?.Name);
                return null;
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
        public async Task<bool> CreateMethodAsync(int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user)
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
                    Description = description,
                    FullCode = methodCode,
                    Status = methodStatus,
                    FileModel = await _context.Files.FindAsync(fileId) ?? throw new NullReferenceException($"Error while creating {methodName} method model: filemodel can't be null")
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
        public async Task<bool> CreateOrUpdateMethodAsync(int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create or update method {MethodName} without admin permissions.", user.Identity?.Name, methodName);
                return false;
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                _logger.LogWarning("Invalid method name {MethodName} provided for creation or update by user {UserName}.", methodName, user.Identity?.Name);
                return false;
            }

            var existingMethod = await _context.Methods.FirstOrDefaultAsync(m => m.FileId == fileId && m.Name == methodName);

            if (existingMethod != null)
            {
                // Update existing method
                return await UpdateMethodAsync(existingMethod.Id, fileId, methodName, description, methodCode, methodStatus, user);
            }
            else
            {
                // Create new method
                return await CreateMethodAsync(fileId, methodName, description, methodCode, methodStatus, user);
            }
        }
        public async Task<bool> UpdateMethodAsync(int methodId, int fileId, string methodName, string? description, string? methodCode, string methodStatus, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to update method {MethodName} without admin permissions.", user.Identity?.Name, methodName);
                return false;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(methodName))
            {
                _logger.LogWarning("Attempted to update method with an invalid name by user {UserName}.", user.Identity?.Name);
                return false;
            }

            if (string.IsNullOrWhiteSpace(methodStatus))
            {
                _logger.LogWarning("Attempted to update method {MethodName} with an invalid status by user {UserName}.", methodName, user.Identity?.Name);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var method = await _context.Methods.FirstOrDefaultAsync(m => m.Id == methodId);
                if (method == null)
                {
                    _logger.LogWarning("Method with ID {MethodId} not found for updating by user {UserName}.", methodId, user.Identity?.Name);
                    return false;
                }

                // Ensure method is associated with the correct file
                if (method.FileId != fileId)
                {
                    _logger.LogWarning("Method with ID {MethodId} does not belong to file with ID {FileId} for updating by user {UserName}.", methodId, fileId, user.Identity?.Name);
                    return false;
                }

                // Update method properties
                method.Name = methodName;
                method.Status = methodStatus;
                method.FullCode = methodCode;
                method.Description = description;

                _context.Methods.Update(method);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Method {MethodName} successfully updated by user {UserName}.", methodName, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to update method {MethodName} by user {UserName}.", methodName, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating the method {MethodName} by user {UserName}.", methodName, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> DeleteMethodAsync(int methodId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to delete method {MethodId} without admin permissions.", user.Identity?.Name, methodId);
                return false;
            }

            if (methodId < 0)
            {
                _logger.LogWarning("Invalid method ID {MethodId} provided for deletion by user {UserName}.", methodId, user.Identity?.Name);
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var method = await _context.Methods.FindAsync(methodId);
                    if (method == null)
                    {
                        _logger.LogWarning("Method with ID {MethodId} not found for deletion by user {UserName}.", methodId, user.Identity?.Name);
                        return false;
                    }

                    _context.Methods.Remove(method);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("Method with ID {MethodId} successfully deleted by user {UserName}.", methodId, user.Identity?.Name);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("No changes detected while attempting to delete method with ID {MethodId} by user {UserName}.", methodId, user.Identity?.Name);
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting method with ID {MethodId} by user {UserName}.", methodId, user.Identity?.Name);
                    return false;
                }
            }
        }
        public async Task<string?> GetMethodCodeAsync(int methodId)
        {

            if (methodId < 0)
            {
                _logger.LogWarning("Invalid method ID {MethodId} provided for getting.", methodId);
                return null;
            }
            try
            {
                // Metódus lekérdezése az adatbázisból
                var method = await _context.Methods.FindAsync(methodId);
                if (method == null)
                {
                    _logger.LogWarning("Method with ID {MethodId} not found .", methodId);
                    return null;
                }
                // Visszaadjuk a metóduskódot
                return method.FullCode;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving method code for method ID {MethodId}}.", methodId);
                return null;
            }
        }
        #endregion
        #region FIELD CRUD
        public async Task<bool> CreateOrUpdateFieldAsync(int fileId, FieldData field, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create or update method {MethodName} without admin permissions.", user.Identity?.Name, field.Name);
                return false;
            }

            if (string.IsNullOrWhiteSpace(field.Name))
            {
                _logger.LogWarning("Invalid field name {FieldName} provided for creation or update by user {UserName}.", field.Name, user.Identity?.Name);
                return false;
            }

            var existingField = await _context.Fields.FirstOrDefaultAsync(f => f.FileId == fileId && f.Name == field.Name);

            if (existingField != null)
            {
                // Update existing method
                return await UpdateFieldAsync(existingField.Id, fileId, field, user);
            }
            else
            {
                // Create new method
                return await CreateFieldAsync(fileId, field, user);
            }
        }
        public async Task<bool> CreateFieldAsync(int fileId, FieldData field, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create method {MethodName} without admin permissions.", user.Identity?.Name, field.Name);
                return false;
            }

            if (string.IsNullOrEmpty(field.Name) || string.IsNullOrWhiteSpace(field.Name))
            {
                _logger.LogWarning("Invalid field name {FieldName} provided for creation by user {UserName}."
                    , field.Name, user.Identity?.Name);
                return false;
            }

            bool isFieldAlreadyExist = await _context.Fields.AnyAsync(f => f.FileId == fileId && f.Name == field.Name);
            if (isFieldAlreadyExist)
            {
                _logger.LogError($"Failed to create {field.Name} field, because there is already a field with this name in the same file.");
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newField = new FieldModel
                {
                    Name = field.Name,
                    FileId = fileId,
                    FileModel = await _context.Files.FindAsync(fileId) ?? throw new NullReferenceException($"Error while creating {field.Name} field model: filemodel can't be null"),
                    Type = field.Type,
                    Description = field.Description,
                    Comment = field.Comment,
                    DefaultValue = field.DefaultValue,
                    IsNullable = field.IsNullable,
                    IsForeignKey = field.IsForeignKey,
                    IsPrimaryKey = field.IsPrimaryKey,
                    ForeignTable = field.ForeignTable
                };

                _context.Fields.Add(newField);
                var result = await _context.SaveChangesAsync();
                
                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Field {FieldName} successfully created by user {UserName}.", field.Name, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create field {FieldName} by user {UserName}.", field.Name, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the method {FieldName} by user {UserName}.", field.Name, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> UpdateFieldAsync(int fileId, int fieldId, FieldData field, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to update field {FieldName} without admin permissions.", user.Identity?.Name, field.Name);
                return false;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(field.Name))
            {
                _logger.LogWarning("Attempted to update field with an invalid name by user {UserName}.", user.Identity?.Name);
                return false;
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingField = await _context.Fields.FirstOrDefaultAsync(f => f.Id == fieldId);
                if (existingField == null)
                {
                    _logger.LogWarning("Field with ID {FieldId} not found for updating by user {UserName}.", fieldId, user.Identity?.Name);
                    return false;
                }

                // Ensure method is associated with the correct file
                if (existingField.FileId != fileId)
                {
                    _logger.LogWarning("Method with ID {MethodId} does not belong to file with ID {FileId} for updating by user {UserName}.", fieldId, fileId, user.Identity?.Name);
                    return false;
                }

                // Update the field's datas
                existingField.Name = field.Name;
                existingField.Description = field.Description;
                existingField.Type = field.Type;
                existingField.IsNullable = field.IsNullable;
                existingField.DefaultValue = field.DefaultValue;
                existingField.ForeignTable = field.ForeignTable;
                existingField.IsForeignKey = field.IsForeignKey;
                existingField.IsPrimaryKey = field.IsPrimaryKey;

                _context.Fields.Update(existingField);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Field {FieldName} successfully updated by user {UserName}.", field.Name, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to update field {FieldName} by user {UserName}.", field.Name, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating the field {FieldName} by user {UserName}.", field.Name, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> DeleteFieldAsync(int fieldId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to delete Field {FieldId} without admin permissions.", user.Identity?.Name, fieldId);
                return false;
            }

            if (fieldId < 0)
            {
                _logger.LogWarning("Invalid Field ID {FieldId} provided for deletion by user {UserName}.", fieldId, user.Identity?.Name);
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var field = await _context.Fields.FindAsync(fieldId);
                    if (field == null)
                    {
                        _logger.LogWarning("Field with ID {FieldId} not found for deletion by user {UserName}.", fieldId, user.Identity?.Name);
                        return false;
                    }

                    _context.Fields.Remove(field);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("Field with ID {FieldId} successfully deleted by user {UserName}.", fieldId, user.Identity?.Name);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("No changes detected while attempting to delete Field with ID {FieldId} by user {UserName}.", fieldId, user.Identity?.Name);
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting Field with ID {FieldId} by user {UserName}.", fieldId, user.Identity?.Name);
                    return false;
                }
            }
        }

        #endregion
        #region MAPPING CRUD
        public async Task<bool> CreateOrUpdateMappingAsync(MappingData mapping, int parentFileId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create or update mapping {MethodName} without admin permissions.", user.Identity?.Name, mapping.RelativePath);
                return false;
            }

            if (string.IsNullOrWhiteSpace(mapping.RelativePath))
            {
                _logger.LogWarning("Invalid mapping name {MappingName} provided for creation or update by user {UserName}.", mapping.RelativePath, user.Identity?.Name);
                return false;
            }

            var existingMapping = await _context.Mappings.FirstOrDefaultAsync(m => m.RelativePath == mapping.RelativePath && m.ParentId == parentFileId);

            if (existingMapping != null)
            {
                if (existingMapping.Code == mapping.Code)
                {
                    //TODO: LOG
                    return true;
                }
                // Update existing method
                return await UpdateMappingAsync(existingMapping.Id, mapping.Code, user);
            }
            else
            {
                // Create new method
                return await CreateMappingAsync(mapping, parentFileId, user);
            }
        }
        public async Task<bool> CreateMappingAsync(MappingData mapping, int parentFileId, ClaimsPrincipal user)
        {
            if (parentFileId < 0)
            {
                _logger.LogWarning("User {UserName} attempted to create mapping {MappingName} but the parentFileId is invalid.", user.Identity?.Name, mapping.RelativePath);
                return false;
            }
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to create method {MethodName} without admin permissions.", user.Identity?.Name, mapping.RelativePath);
                return false;
            }

            if (string.IsNullOrEmpty(mapping.RelativePath) || string.IsNullOrWhiteSpace(mapping.RelativePath))
            {
                _logger.LogWarning("Invalid Mapping name {MappingName} provided for creation by user {UserName}."
                    , mapping.RelativePath, user.Identity?.Name);
                return false;
            }

            bool isMappingAlreadyExist = await _context.Mappings.AnyAsync(m => m.RelativePath == mapping.RelativePath && m.ParentId == parentFileId);
            if (isMappingAlreadyExist)
            {
                _logger.LogError($"Failed to create {mapping.RelativePath} Mapping, because there is already a Mapping with this name in the same folder.");
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newMapping = new MappingModel
                {
                    RelativePath = mapping.RelativePath,
                    Code = mapping.Code,
                    IsNew = mapping.IsNew,
                    ParentEntitysName = mapping.ParentEntitysName,
                    ParentId = parentFileId
                };

                _context.Mappings.Add(newMapping);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Mapping {MappingName} successfully created by user {UserName}.", mapping.RelativePath, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to create Mapping {MappingName} by user {UserName}.", mapping.RelativePath, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating the method {MappingName} by user {UserName}.", mapping.RelativePath, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> UpdateMappingAsync(int mappingId, string code, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to update mapping {FieldName} without admin permissions.", user.Identity?.Name, mappingId);
                return false;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning("Attempted to update field with an invalid name by user {UserName}.", user.Identity?.Name);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingMapping = await _context.Mappings.FirstOrDefaultAsync(f => f.Id == mappingId);
                if (existingMapping == null)
                {
                    _logger.LogWarning("Field with ID {FieldId} not found for updating by user {UserName}.", existingMapping.Id, user.Identity?.Name);
                    return false;
                }

                // Update the field's datas
                existingMapping.Code = code;

                _context.Mappings.Update(existingMapping);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Field {FieldName} successfully updated by user {UserName}.", existingMapping.RelativePath, user.Identity?.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes detected while attempting to update field {FieldName} by user {UserName}.", existingMapping.RelativePath, user.Identity?.Name);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating the field {FieldName} by user {UserName}.", mappingId, user.Identity?.Name);
                return false;
            }
        }
        public async Task<bool> DeleteMappingAsync(int mappingId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to delete Mapping {MappingId} without admin permissions.", user.Identity?.Name, mappingId);
                return false;
            }

            if (mappingId < 0)
            {
                _logger.LogWarning("Invalid Mapping ID {MappingId} provided for deletion by user {UserName}.", mappingId, user.Identity?.Name);
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var mapping = await _context.Mappings.FindAsync(mappingId);
                    if (mapping == null)
                    {
                        _logger.LogWarning("Mapping with ID {MappingId} not found for deletion by user {UserName}.", mappingId, user.Identity?.Name);
                        return false;
                    }

                    _context.Mappings.Remove(mapping);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("Mapping with ID {MappingId} successfully deleted by user {UserName}.", mappingId, user.Identity?.Name);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("No changes detected while attempting to delete Mapping with ID {MappingId} by user {UserName}.", mappingId, user.Identity?.Name);
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting Mapping with ID {MappingId} by user {UserName}.", mappingId, user.Identity?.Name);
                    return false;
                }
            }
        }

        #endregion

        #region SEARCH
        public async Task<int?> GetFolderIdByNameAndParentId(string name, int? parentId)
        {
            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Name.Trim().ToLower() == name.Trim().ToLower() && f.ParentId == parentId);

            return folder?.Id;
        }
        public async Task<bool> FileExistsAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // Split path into segments using the normalized separator
            var pathSegments = filePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                _logger.LogWarning($"Nem sikerült az útvonal szétdarabolása");
                return false; // Nem érvényes fájlútvonal
            }

            int? parentId = null;
            bool folderFound = true;

            // Végigmegyünk az útvonal mappáin
            for (int i = 0; i < pathSegments.Length - 1; i++)
            {
                var folderName = pathSegments[i];
                var folderId = await GetFolderIdByNameAndParentId(folderName, parentId);
                // Ha a mappastruktúra nem létezik, visszatérünk false-szal
                if (folderId == null)
                    return false;

                // Frissítjük a parentId-t a következő szinthez
                parentId = folderId;
            }
            // Ellenőrizzük, hogy a fájl létezik-e az utolsó szint mappájában
            var fileNameWithExtension = pathSegments.Last();
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension); // Kiterjesztés eltávolítása

            return await _context.Files.AnyAsync(f => f.Name.Trim().ToLower() == fileName.Trim().ToLower() && f.ParentId == parentId);

        }
        public async Task<FileModel?> GetFileByPathAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            // Split path into segments using the normalized separator
            var pathSegments = filePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                _logger.LogWarning($"Nem sikerült az útvonal szétdarabolása");
                return null; // Nem érvényes fájlútvonal
            }

            int? parentId = null;
            // Végigmegyünk az útvonal mappáin
            for (int i = 0; i < pathSegments.Length - 1; i++)
            {
                var folderName = pathSegments[i];
                var folderId = await GetFolderIdByNameAndParentId(folderName, parentId);
                // Ha a mappastruktúra nem létezik, visszatérünk false-szal
                if (folderId == null)
                    return null;

                // Frissítjük a parentId-t a következő szinthez
                parentId = folderId;
            }
            // Ellenőrizzük, hogy a fájl létezik-e az utolsó szint mappájában
            var fileNameWithExtension = pathSegments.Last();
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension); // Kiterjesztés eltávolítása

            return await _context.Files
                .Include(f => f.Methods)
                .FirstOrDefaultAsync(f => f.Name.Trim().ToLower() == fileName.Trim().ToLower() && f.ParentId == parentId);
        }
        private async Task<string> GetFilePathAsync(int fileId)
        {
            var file = await _context.Files.Include(f => f.ParentFolder).FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null || file.ParentFolder == null)
            {
                _logger.LogWarning("File with ID {FileId} not found or has no parent folder.", fileId);
                return null; // Ha a fájl vagy a szülő mappa nem található
            }

            // Kezdjük a file nevével
            var fullPath = "";
            var currentFolder = file.ParentFolder;

            // Végigmegyünk az összes szülő mappán
            while (currentFolder != null)
            {
                fullPath = $"{currentFolder.Name}/{fullPath}";
                currentFolder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == currentFolder.ParentId);
            }

            return fullPath;
        }

        #endregion

        #region DATABASE
        public async Task<DatabaseViewModel> GetDatabaseViewModelAsync()
        {
            try
            {
                // Get database entitites
                var databaseEntities = await _context.Files
                    .Include(f => f.Fields)
                    .Where(f => f.IsDatabaseEntity)
                    .ToListAsync() 
                    ?? new List<FileModel>();

                return new DatabaseViewModel
                {
                    Entities = databaseEntities
                };
            }
            catch (Exception ex)
            {
                // Hiba naplózása
                _logger.LogError(ex, "Error while fetching database entities.");
                throw; // A hibát továbbra is feldobjuk, hogy a hívó kezelhesse
            }
        }

        public async Task<FileModel?> GetDatabaseEntityById(int id) 
        {
            try
            {
                // Get database entitites
                var entity = await _context.Files
                    .Include(f => f.Fields)
                    .FirstOrDefaultAsync(f => f.IsDatabaseEntity && f.Id == id);

                return entity;
            }
            catch (Exception ex)
            {
                // Hiba naplózása
                _logger.LogError(ex, "Error while fetching database entities.");
                throw; // A hibát továbbra is feldobjuk, hogy a hívó kezelhesse
            }
        }
        public async Task UpdateDatabaseEntityAsync(int id, FileModel entity)
        {
            // Keresd meg az entitást az adatbázisban
            var existingEntity = await _context.Files.Include(f => f.Fields)
                                                     .FirstOrDefaultAsync(f => f.Id == id);

            if (existingEntity == null)
            {
                throw new ArgumentException($"Entity with ID {id} not found.");
            }

            // Frissítsd az alapvető mezőket
            //existingEntity.Name = entity.Name;
            existingEntity.Description = entity.Description;
            _logger.LogInformation($"existingEntity.Description: {existingEntity.Description} entity.Description: {entity.Description}");
            //existingEntity.Status = entity.Status;

            // Frissítsd a mezőket
            foreach (var updatedField in entity.Fields)
            {
                var existingField = existingEntity.Fields.FirstOrDefault(f => f.Id == updatedField.Id);

                if (existingField != null)
                {
                    // Ha a mező létezik, frissítsd az értékeket
                    //existingField.Name = updatedField.Name;
                    //existingField.Type = updatedField.Type;
                    existingField.Description = updatedField.Description;
                    //existingField.IsNullable = updatedField.IsNullable;
                    //existingField.DefaultValue = updatedField.DefaultValue;
                    //existingField.Comment = updatedField.Comment;
                    //existingField.IsPrimaryKey = updatedField.IsPrimaryKey;
                    //existingField.IsForeignKey = updatedField.IsForeignKey;
                    //existingField.ForeignTable = updatedField.ForeignTable;
                }
                else
                {
                    // Ha a mező nem létezik, add hozzá
                    existingEntity.Fields.Add(updatedField);
                }
            }

            // Távolítsd el azokat a mezőket, amelyek nincsenek az új entitásban
            var updatedFieldIds = entity.Fields.Select(f => f.Id).ToList();
            var fieldsToRemove = existingEntity.Fields.Where(f => !updatedFieldIds.Contains(f.Id)).ToList();
            foreach (var field in fieldsToRemove)
            {
                existingEntity.Fields.Remove(field);
            }
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to the database.");
                throw;
            }
            // Mentés az adatbázisba
        }

        #endregion

        #region EXPORT
        public async Task<List<ExportFileDto>> GetFilesForExportAsync()
        {
            var exportData = new List<ExportFileDto>();

            var files = await _context.Files
                .Where(f => !f.IsEntity && !f.IsMapping)
                .Include(f => f.Methods) // Tartalmazza a metódusokat
                .ToListAsync();

            foreach (var file in files)
            {
                var filePath = await GetFilePathAsync(file.Id); // Aszinkron hívás

                exportData.Add(new ExportFileDto
                {
                    Path = filePath,
                    Status = file.Status,
                    Description = file.Description,
                    Methods = file.Methods.Select(method => new ExportMethodDto
                    {
                        Name = method.Name,
                        Description = method.Description,
                        Code = method.FullCode,
                        Status = method.Status
                    }).ToList()
                });
            }

            return exportData;

        }
        public async Task<List<ExportEntityDto>> GetEntitiesForExportAsync()
        {
            var exportData = new List<ExportEntityDto>();

            var entities = await _context.Files
                .Where(f => f.IsEntity)
                .Include(f => f.Fields)
                .ToListAsync();

            foreach (var entity in entities)
            {
                var filePath = await GetFilePathAsync(entity.Id); // Aszinkron hívás

                exportData.Add(new ExportEntityDto
                {
                    Path = filePath,
                    Status = entity.Status,
                    Description = entity.Description,
                    Fields = entity.Fields.Select(field => new ExportFieldDto
                    {
                        Name = field.Name,
                        Description = field.Description,
                        Type = field.Type,
                        IsForeignKey = field.IsForeignKey,
                        IsPrimaryKey = field.IsPrimaryKey,
                        IsNullable = field.IsNullable
                    }).ToList()
                });
            }

            return exportData;

        }
        #endregion
    }
}
