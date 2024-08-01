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
                _logger.LogInformation("Fetching root folders");
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

        //public async Task<IEnumerable<FolderModel>> GetSubFoldersAsync(int parentId)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Fetching subfolders for parent ID {ParentId}", parentId);
        //        return await _context.Folders
        //            .Where(f => f.ParentId == parentId)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching subfolders for parent ID {ParentId}", parentId);
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<FolderModel>> GetFoldersAsync(int parentId)
        {
            try
            {
                _logger.LogInformation("Fetching folders for parent ID {ParentId}", parentId);
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
    }
}
