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
                FileName = file.Name,
                Methods = file.Methods
            };

            return fileContent;
        }
        public async Task<bool> DeleteFolderAsync(int folderId, ClaimsPrincipal user)
        {
            if (!user.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserName} attempted to delete folder {FolderId} without admin permissions.", user.Identity.Name, folderId);
                return false;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var folder = await _context.Folders.FindAsync(folderId);
                    if (folder == null)
                    {
                        _logger.LogWarning("Folder with ID {FolderId} not found for deletion by user {UserName}.", folderId, user.Identity.Name);
                        return false;
                    }

                    _context.Folders.Remove(folder);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("Folder with ID {FolderId} successfully deleted by user {UserName}.", folderId, user.Identity.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting folder with ID {FolderId} by user {UserName}.", folderId, user.Identity.Name);
                    return false;
                }
            }
        }

    }
}
