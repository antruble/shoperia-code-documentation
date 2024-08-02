using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using ShoperiaDocumentation.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ShoperiaDocumentation.Controllers
{
    public class ClassTreeController : Controller
    {
        private readonly IFileService _fileService;
        private readonly ILogger<ClassTreeController> _logger;

        public ClassTreeController(IFileService fileService, ILogger<ClassTreeController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var rootFolders = await _fileService.GetRootFoldersAsync();
                return View(rootFolders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching root folders");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int parentId)
        {
            try
            {
                var subFolders = await _fileService.GetFoldersAsync(parentId);
                return Json(subFolders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subfolders for parent ID {ParentId}", parentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFolders(int parentId)
        {
            try
            {
                var folders = await _fileService.GetFoldersAsync(parentId);
                _logger.LogInformation($"GETFOLDERS: {folders.Count()}");
                return PartialView("_FolderListPartial", folders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folders for parent ID {ParentId}", parentId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
