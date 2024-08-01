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
        public async Task<IEnumerable<FolderModel>> GetMainCategoriesAsync()
        {
            
            try
            {
                return await _context.Folders
                .Where(f => f.ParentId == null)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching main categories");
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
        public async Task<int> GetFolderIdByPathAsync(string path)
        {
            try
            {
                _logger.LogInformation("Fetching folder ID for path {Path}", path);
                var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                FolderModel currentFolder = null;

                foreach (var segment in segments)
                {
                    currentFolder = await _context.Folders
                        .FirstOrDefaultAsync(f => f.Name == segment && (currentFolder == null ? f.ParentId == null : f.ParentId == currentFolder.Id));

                    if (currentFolder == null)
                    {
                        throw new Exception($"Folder not found for path segment {segment}");
                    }
                }

                return currentFolder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folder ID for path {Path}", path);
                throw;
            }
        }
    }
}
