using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoperiaDocumentation.Services;

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
            _logger.LogInformation($"Sikeres CheckFileExists hívás. Path: {path}");
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("File name cannot be empty.");
            }

            var fileExists = await _fileService.FileExistsAsync(path);
            _logger.LogInformation($"CheckFileExists hívás eredménye: {fileExists}");
            return Ok(fileExists);
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
                return Ok(new { message = "JSON data processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
