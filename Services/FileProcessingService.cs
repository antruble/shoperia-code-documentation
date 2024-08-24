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
                int parentId = -1;
                if (directory != null)
                    parentId = await CreateDirectories(directory, user);
                if (parentId == -1)
                {
                    _logger.LogError($"Something went wrong at the CreateDirectories method while processed {filePath} path.");
                    continue;
                }
                // Fájl létrehozása vagy frissítése
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                if (fileName == null)
                {
                    _logger.LogError($"Something went wrong while tried to get file name from the path: {filePath}");
                    continue;
                }
                int? fileId = await _fileService.CreateFileAsync(fileName, file.Status, parentId, user);
                if (fileId == null)
                {
                    _logger.LogError($"Something went wrong while tried to create the file from the path: {filePath}");
                    continue;
                }

                foreach (var method in file.Methods)
                {
                    // Metódus létrehozása vagy frissítése
                    _logger.LogInformation($"Processing method: {method.Name} in file: {filePath}");
                    await _fileService.CreateOrUpdateMethodAsync(fileId ?? -1, method.Name, method.Description, method.Code, method.Status, user);
                }
            }
        }
        public async Task<int> CreateDirectories(string? path, ClaimsPrincipal user) 
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
                            return -1;
                        }
                        parentId = await _fileService.CreateFolderAsync(dir, "new", parentId, user);
                        continue;
                    }
                    parentId = folderId;
                }
                return parentId ?? -1;
            }
            return -1;
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
            [JsonProperty("status")]
            public string Status { get; set; }

        }
    }
}
