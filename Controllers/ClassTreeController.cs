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

        public async Task<IActionResult> Index(string path = "")
        {
            try
            {
                _logger.LogInformation($"PATH:: {path}");
                var result = await _fileService.GetDataFromUrlAsync(path);
                _logger.LogInformation($"RESUTL:: {result}");
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folders for path {Path}", path);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        public async Task<FolderViewModel> GetDataByUrl(string path)
        {
            var result = await _fileService.GetDataFromUrlAsync(path);
            return result;
        }




        [HttpGet]
        public async Task<IActionResult> GetSubCategories(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                _logger.LogError("Path is null or empty");
                return BadRequest("Path cannot be null or empty");
            }

            try
            {
                _logger.LogInformation("Fetching subcategories for path: {Path}", path);
                var subFolders = await _fileService.GetFoldersByPathAsync(path);
                ViewBag.CurrentPath = path;
                return Json(subFolders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subfolders for path {Path}", path);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFolders(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                _logger.LogError("Path is null or empty");
                return BadRequest("Path cannot be null or empty"); // Use BadRequest to return a proper error message
            }

            try
            {
                var folders = await _fileService.GetFoldersByPathAsync(path);
                return PartialView("_FolderListPartial", folders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folders for path {Path}", path);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
