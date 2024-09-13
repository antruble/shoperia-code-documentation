using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoperiaDocumentation.Services;
namespace ShoperiaDocumentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JsonUploadController : ControllerBase
    {
        private readonly IFileProcessingService _fileProcessingService;

        public JsonUploadController(IFileProcessingService fileProcessingService)
        {
            _fileProcessingService = fileProcessingService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadJsonFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file was uploaded.");
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var reader = new StreamReader(stream))
                    {
                        var jsonData = await reader.ReadToEndAsync();

                        Console.WriteLine($"JSONDATA: {jsonData}");
                        await _fileProcessingService.ProcessJsonAsync(jsonData, User);
                    }
                }

                return Ok(new { message = "File uploaded and processed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
