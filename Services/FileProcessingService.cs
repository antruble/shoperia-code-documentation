using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ShoperiaDocumentation.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileProcessingService> _logger;

        public FileProcessingService(IFileService fileService, ILogger<FileProcessingService> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        public async Task ProcessJsonAsync(string jsonData, ClaimsPrincipal user)
        {
            _logger.LogInformation("LEFUT A ProcessJsonAsync METÓDUS");

            var rootData = JsonConvert.DeserializeObject<RootData>(jsonData);
            if (rootData?.Files == null)
            {
                _logger.LogError("No files found in JSON data.");
                return;
            }

            foreach (var file in rootData.Files)
            {
                string filePath = file.Path;
                _logger.LogInformation($"Processing file: {filePath}");

                // Mappák létrehozása, ha nem léteznek
                var directory = Path.GetDirectoryName(filePath);
                if (directory != null)
                    await CreateDirectories(directory, user);

                // Fájl létrehozása vagy frissítése
                //await _fileService.CreateOrUpdateFileAsync(filePath, file.Status, user);

                foreach (var method in file.Methods)
                {
                    // Metódus létrehozása vagy frissítése
                    _logger.LogInformation($"Processing method: {method.Name} in file: {filePath}");
                    //await _fileService.CreateOrUpdateMethodAsync(filePath, method, user);
                }
            }
        }
        public async Task CreateDirectories(string? path, ClaimsPrincipal user) 
        {
            if (path != null)
            {
                // Felbontjuk az útvonalat mappákra
                var directories = path.Split(Path.DirectorySeparatorChar);

                string currentPath = string.Empty;
                int? parentId = null;
                foreach (var dir in directories)
                {
                    int? folderId = await _fileService.GetFolderIdByNameAndParentId(dir, parentId);
                    if (folderId == null)
                    {
                        if (parentId == -1)
                        {
                            _logger.LogError($"Root directory is missing with the name of {dir}!");
                            return;
                        }
                        parentId = await _fileService.CreateFolderAsync(dir, "new", parentId, user);
                        continue;
                    }
                    parentId = folderId;
                }
            }
        }

        public class RootData
        {
            [JsonProperty("files")]
            public List<FileData> Files { get; set; }
        }

        public class FileData
        {
            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; } // "New" or "Modified"

            [JsonProperty("methods")]
            public List<MethodData> Methods { get; set; }
        }

        public class MethodData
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }
        }
    }
}
