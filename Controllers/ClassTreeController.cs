using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using ShoperiaDocumentation.Models.Requests;
using ShoperiaDocumentation.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ShoperiaDocumentation.Controllers
{
    public class ClassTreeController : Controller
    {
        private readonly IFileService _fileService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ClassTreeController> _logger;

        public ClassTreeController(IFileService fileService, UserManager<IdentityUser> userManager, ILogger<ClassTreeController> logger)
        {
            _fileService = fileService;
            _userManager = userManager;
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
        public async Task<IActionResult> GetFileContent(int fileId, bool isEntity = false, bool isMapping = false, bool isDatabaseEntity = false)
        {
            try
            {
                var fileContent = await _fileService.GetFileContentAsync(fileId, isEntity: isEntity, isMapping: isMapping, isDatabaseEntity: isDatabaseEntity);
                return PartialView("_FileContentModalPartial", fileContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching file content for fileId {FileId}", fileId);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFolderOrFile([FromBody] DeleteItemRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            var success = await _fileService.DeleteFolderAsync(request.ItemId, User);
            if (success)
            {
                _logger.LogInformation("Folder with ID {FolderId} deleted by user {UserId}.", request.ItemId, user?.UserName);
                return Ok();
            }
            else
            {
                _logger.LogWarning("Failed to delete folder with ID {FolderId} by user {UserId}.", request.ItemId, user?.UserName);
                return BadRequest("Failed to delete folder.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RenameFolderOrFile([FromBody] RenameItemRequest request)
        {
            bool success = false;
            if (request.Type == "folder")
                success = await _fileService.RenameFolderAsync(request.ItemId, request.NewName, User);
            else if(request.Type == "file")
                success = await _fileService.RenameFileAsync(request.ItemId, request.NewName, User);
            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to rename folder.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFolderOrFile([FromBody] CreateItemRequest request)
        {
            bool success = false;
            if (request.Type == "folder")
                success = await _fileService.CreateFolderAsync(request.Name, request.Status, request.ParentId, User) != null;
            else if(request.Type == "file")
                success = await _fileService.CreateFileAsync(request.Name, request.Status, request.ParentId, User) != null;
            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to rename folder.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMethod([FromBody] CreateMethodRequest request)
        {
            bool success = await _fileService.CreateMethodAsync(
                request.FileId,
                request.Name,
                request.Description,  
                request.Code,
                request.Status,
                User
            );

            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to create method.");
            }
        }
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditMethod(int id, [FromBody] CreateMethodRequest request)
        {
            _logger.LogInformation($"request.FileId: {request.FileId} request.Name: {request.Name} request.Description: {request.Description} request.Code: {request.Code} request.Status: {request.Status}");
            bool success = await _fileService.UpdateMethodAsync(id, request.FileId, request.Name, request.Description, request.Code, request.Status, User);
            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to update method.");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMethod(int id)
        {
            bool success = await _fileService.DeleteMethodAsync(id, User);
            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Failed to update method.");
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMethodCode(int id)
        {
            var result = await _fileService.GetMethodCodeAsync(id);
            if (result != null)
            {
                return Ok(new { code = result }); // A kódot JSON objektumként adja vissza
            }
            else
            {
                return BadRequest("Failed to get method code.");
            }
        }

    }
}
