using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoperiaDocumentation.Models;
using ShoperiaDocumentation.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ShoperiaDocumentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileService _fileService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IFileService fileService,
            ILogger<HomeController> logger
            
            )
        {
            _fileService = fileService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "ClassTree");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> ExportDatabaseAsync()
        {
            try
            {
                var exportData = await _fileService.GetFilesForExportAsync(); // Szolgáltatás hívása az exportálási adatokért

                var json = JsonConvert.SerializeObject(new { files = exportData }, Formatting.Indented);

                // Fájl generálása és letöltés indítása
                var fileName = "database-export.json";
                var contentType = "application/json";
                var fileBytes = Encoding.UTF8.GetBytes(json);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export database.");
                return StatusCode(500, "An error occurred while exporting the database.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ExportEntitiesAsync()
        {
            try
            {
                var exportData = await _fileService.GetEntitiesForExportAsync(); // Szolgáltatás hívása az exportálási adatokért

                var json = JsonConvert.SerializeObject(new { files = exportData }, Formatting.Indented);

                // Fájl generálása és letöltés indítása
                var fileName = "entities-export.json";
                var contentType = "application/json";
                var fileBytes = Encoding.UTF8.GetBytes(json);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export entities.");
                return StatusCode(500, "An error occurred while exporting the entities.");
            }
        }

    }
}