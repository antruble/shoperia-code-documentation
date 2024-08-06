using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using ShoperiaDocumentation.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
                var result = await _fileService.GetFolderHierarchyFromPathAsync(path);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching folders for path {Path}", path);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetFileContent(int fileId)
        {
            try
            {
                var fileContent = await _fileService.GetFileContentAsync(fileId);
                return PartialView("_FileContentModalPartial", fileContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching file content for fileId {FileId}", fileId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
