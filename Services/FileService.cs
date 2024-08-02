using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;

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

        public async Task<IEnumerable<FolderModel>> GetRootFoldersAsync()
        {
            try
            {
                return await _context.Folders
                    .Where(f => f.ParentId == null)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching root folders");
                throw;
            }
        }
        public async Task<IEnumerable<FolderModel>> GetFoldersAsync(int parentId)
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
        public async Task<IEnumerable<FolderModel>> GetFoldersByPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                _logger.LogError("Path is null or empty");
                return Enumerable.Empty<FolderModel>();
            }
            var pathParts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
            FolderModel parentFolder = null;

            foreach (var part in pathParts)
            {
                parentFolder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Name == part && (parentFolder == null ? f.ParentId == null : f.ParentId == parentFolder.Id));

                if (parentFolder == null)
                {
                    _logger.LogError("Folder not found for path part {PathPart}", part);
                    return Enumerable.Empty<FolderModel>();
                }
            }

            return parentFolder == null
                ? await GetRootFoldersAsync()
                : await GetFoldersAsync(parentFolder.Id);
        }
    }
}
