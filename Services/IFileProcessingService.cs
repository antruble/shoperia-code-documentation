using System.Security.Claims;

namespace ShoperiaDocumentation.Services
{
    public interface IFileProcessingService
    {
        Task ProcessJsonAsync(string jsonData, ClaimsPrincipal user);

        Task ProcessDatabaseJsonAsync(string jsonData, ClaimsPrincipal user);
    }
}
