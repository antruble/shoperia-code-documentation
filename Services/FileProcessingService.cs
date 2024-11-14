using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ShoperiaDocumentation.Models;
using static ShoperiaDocumentation.Services.FileProcessingService;

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
                var tempFileModel = new FileModel 
                { 
                    Name = fileName,
                    Description = string.IsNullOrEmpty(file.Description) ? string.Empty : file.Description,
                    Status = file.Status,
                    ParentId = parentId
                };
                int? fileId = await _fileService.CreateFileAsync(tempFileModel, user);
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
        public async Task ProcessDatabaseJsonAsync(string jsonData, ClaimsPrincipal user)  //TODO: PROCESS ENTITIES
        {
            var rootData = JsonConvert.DeserializeObject<RootEntityData>(jsonData);
            if (rootData?.Entities == null)
            {
                _logger.LogError("No entities found in JSON data.");
                return;
            }

            foreach (var entity in rootData.Entities)
            {
                string entityPath = entity.RelativePath;
                _logger.LogInformation($"Processing entity: {entityPath}");

                // Mappák létrehozása, ha nem léteznek
                var directory = Path.GetDirectoryName(entityPath);
                int parentId = -1;
                if (directory != null)
                    parentId = await CreateDirectories(directory, user);
                if (parentId == -1)
                {
                    _logger.LogError($"Something went wrong at the CreateDirectories method while processed {entityPath} path.");
                    continue;
                }
                // Fájl létrehozása vagy frissítése
                var fileName = Path.GetFileNameWithoutExtension(entityPath);

                if (fileName == null)
                {
                    _logger.LogError($"Something went wrong while tried to get entity name from the path: {entityPath}");
                    continue;
                }
                var status = entity.IsNew ? "new" : "modified";
                int? fileId = await _fileService.CreateFileAsync(fileName, status, parentId, user, isEntity: true, isDatabaseEntity: entity.IsDatabaseEntity);
                if (fileId == null)
                {
                    _logger.LogError($"Something went wrong while tried to create the entity with the {entityPath} path");
                    continue;
                }
                // Create fields
                foreach (var field in entity.Fields)
                {
                    // Metódus létrehozása vagy frissítése
                    _logger.LogInformation($"Processing field: {field.Name}");
                    await _fileService.CreateOrUpdateFieldAsync(fileId ?? -1, field, user);
                }
                if (entity.Mapping != null) //TODO CREATE MAPPING
                {
                    _logger.LogInformation($"Processing mapping for the entity: {entityPath}");

                    var directoryForMapping = Path.GetDirectoryName(entity.Mapping.RelativePath);
                    int parentIdForMapping = -1;
                    if (directoryForMapping != null)
                        parentIdForMapping = await CreateDirectories(directoryForMapping, user);
                    if (parentIdForMapping == -1)
                    {
                        _logger.LogError($"Something went wrong at the CreateDirectories method while processed {entity.Mapping.RelativePath} path.");
                        continue;
                    }
                    var fileNameForMapping = Path.GetFileNameWithoutExtension(entity.Mapping.RelativePath);
                    var statusForMapping = entity.Mapping.IsNew ? "new" : "modified";
                    
                    var mappingFileId = await _fileService.CreateFileAsync(fileNameForMapping, statusForMapping, parentIdForMapping, user, isMapping: true) ?? -1;
                    await _fileService.CreateMappingAsync(entity.Mapping, mappingFileId, user);

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
            
            [JsonProperty("description")]
            public string? Description { get; set; }

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

        //entity
        public class RootEntityData
        {
            [JsonProperty("entities")]
            public List<EntityData> Entities { get; set; }
        }

        public class EntityData
        {
            [JsonProperty("RelativePath")]
            public string RelativePath { get; set; }

            [JsonProperty("IsNew")]
            public bool IsNew { get; set; } // "New" or "Modified"

            [JsonProperty("IsDatabaseEntity")]
            public bool IsDatabaseEntity { get; set; }

            [JsonProperty("Fiels")]
            public List<FieldData>? Fields{ get; set; }

            [JsonProperty("Mapping")]
            public MappingData? Mapping { get; set; }
        }

        public class FieldData
        {
            [JsonProperty("Name")]
            public required string Name { get; set; }

            [JsonProperty("Type")]
            public required string Type { get; set; }

            [JsonProperty("Description")]
            public required string Description { get; set; } = string.Empty;

            [JsonProperty("IsNullable")]
            public bool IsNullable{ get; set; }
            
            [JsonProperty("DefaultValue")]
            public string? DefaultValue { get; set; }

            [JsonProperty("Comment")]
            public string? Comment { get; set; }

            [JsonProperty("IsPrimaryKey")]
            public bool IsPrimaryKey { get; set; }

            [JsonProperty("IsForeignKey")]
            public bool IsForeignKey { get; set; }

            [JsonProperty("ForeignTable")]
            public string? ForeignTable { get; set; }
        }
        public class MappingData
        {
            [JsonProperty("RelativePath")]
            public required string RelativePath { get; set; }

            [JsonProperty("ParentEntitysName")]
            public required string ParentEntitysName { get; set; }

            [JsonProperty("Code")]
            public string? Code { get; set; }

            [JsonProperty("IsNew")]
            public required bool IsNew { get; set; }
        }
    }
}
