using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Models.Requests;
using ShoperiaDocumentation.Models.ViewModels;
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
        public async Task<IActionResult> GetEntityPartial(int id)
        {
            var entity = await _fileService.GetDatabaseEntityById(id);

            if (entity == null)
            {
                return NotFound(new { message = "Entity not found" });
            }

            var model = new EntityViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Fields = entity.Fields.Select(f => new FieldViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Type = f.Type,
                    Description = f.Description
                }).ToList()
            };

            return PartialView("_EntityContentPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDatabaseEntity([FromBody] UpdateEntityRequest request)
        {
            _logger.LogInformation($"rquest dsc {request.Description}");
            if (request == null || request.Id <= 0)
            {
                return BadRequest("Invalid entity data.");
            }

            var entity = await _fileService.GetDatabaseEntityById(request.Id);
            if (entity == null)
            {
                return NotFound("Entity not found.");
            }

            // Frissítsük az entitás leírását
            entity.Description = request.Description;

            // Frissítsük a mezők leírásait
            foreach (var updatedField in request.Fields)
            {
                var field = entity.Fields.FirstOrDefault(f => f.Name == updatedField.Name);
                if (field != null)
                {
                    field.Description = updatedField.Description;
                }
            }
            await _fileService.UpdateDatabaseEntityAsync(request.Id, entity);
            return Ok();
        }
    }
}
