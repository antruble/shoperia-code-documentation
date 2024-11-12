using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Services;

namespace ShoperiaDocumentation.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly IFileService _fileService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ClassTreeController> _logger;

        public DatabaseController(IFileService fileService, UserManager<IdentityUser> userManager, ILogger<ClassTreeController> logger)
        {
            _fileService = fileService;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var model = await _fileService.GetDatabaseViewModelAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetEntityDetails(int id)
        {
            var entity = await _fileService.GetDatabaseEntityById(id);

            if (entity == null)
            {
                return NotFound(new { message = "Entity not found" });
            }

            // JSON válasz
            return Ok(new
            {
                id = entity.Id,
                name = entity.Name,
                isNew = entity.Status.ToLower() == "new",
                fields = entity.Fields
            });
        }

    }
}
