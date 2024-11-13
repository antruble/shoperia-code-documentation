using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoperiaDocumentation.Models.Requests;
using ShoperiaDocumentation.Services;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace ShoperiaDocumentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(IFileService fileService, IFileProcessingService fileProcessingService, ILogger<ApiController> logger)
        {
            _fileService = fileService;
            _fileProcessingService = fileProcessingService;
            _logger = logger;
        }

        // API végpont, amely ellenőrzi, hogy egy fájl létezik-e az adatbázisban
        [HttpGet("CheckFileExists")]
        public async Task<IActionResult> CheckFileExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("File name cannot be empty.");
            }

            var fileExists = await _fileService.FileExistsAsync(path);
            _logger.LogInformation($"Sikeres CheckFileExists hívás az alábbi útvonalhoz: {path}! Eredmény: {fileExists}");
            return Ok(fileExists);
        }
        // API végpont, amely ellenőrzi, hogy egy fájl létezik-e az adatbázisban
        [HttpGet("IsMethodRegistered")]
        public async Task<IActionResult> IsMethodRegistered(string filePath, string methodName)
        {
            _logger.LogInformation($"IsMethodRegistered végpont meghívva a filepath: {filePath} methodname: {methodName} paraméterekkel!");
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(methodName))
                return BadRequest("File path or method name is required!");

            var file = await _fileService.GetFileByPathAsync(filePath);
            if (file == null)
                return Ok(false);
            var isMethodRegistered = file.Methods.Where(m => m.Name == methodName).Any();

            _logger.LogInformation($"Sikeresen lefutott az IsMethodRegistered api hívás, aminek az eredménye: {isMethodRegistered}");
            return Ok(isMethodRegistered);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ProcessDatabaseEntity")]
        public async Task<IActionResult> ProcessDatabaseEntities([FromBody] FileProcessingService.RootEntityData jsonData)
        {
            if (jsonData == null || jsonData.Entities == null || jsonData.Entities.Count == 0)
            {
                return BadRequest("Invalid or empty JSON data.");
            }
            _logger.LogInformation($"Beérkező ProcessDatabaseEntities hívás");
            try
            {
                await _fileProcessingService.ProcessDatabaseJsonAsync(JsonConvert.SerializeObject(jsonData), User);
                _logger.LogInformation($"Sikeresen feltöltve {jsonData.Entities.Count} darab adatbázis entitás!");
                return Ok(new { message = "JSON data processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ProcessJsonData")]
        public async Task<IActionResult> ProcessJsonData([FromBody] FileProcessingService.RootData jsonData)
        {
            if (jsonData == null || jsonData.Files == null || jsonData.Files.Count == 0)
            {
                return BadRequest("Invalid or empty JSON data.");
            }

            try
            {
                await _fileProcessingService.ProcessJsonAsync(JsonConvert.SerializeObject(jsonData), User);
                _logger.LogInformation($"Sikeresen feltöltve {jsonData.Files.Count} darab fájl!");
                return Ok(new { message = "JSON data processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("UpdateField")]
        public async Task<IActionResult> UpdateField([FromBody] UpdateFieldRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // Frissítsük az adott mezőt
                var fieldData = new FileProcessingService.FieldData { Description = request.Description, Name = request.Name, Type = request.Type };
                var isUpdated = await _fileService.UpdateFieldAsync(request.FileId, request.FieldId, fieldData, User);

                if (!isUpdated)
                {
                    return NotFound($"Field with ID {request.FieldId} not found.");
                }

                _logger.LogInformation($"Field description updated successfully for Field ID: {request.FieldId}.");
                return Ok(new { message = "Field description updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating field description.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

    }
}
