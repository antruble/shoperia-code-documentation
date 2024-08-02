using Microsoft.AspNetCore.Mvc;
using ShoperiaDocumentation.Services;
using System.Threading.Tasks;

namespace ShoperiaDocumentation.ViewComponents
{
    public class CTMainCategoriesViewComponent : ViewComponent
    {
        private readonly IFileService _fileService;

        public CTMainCategoriesViewComponent(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var mainCategories = await _fileService.GetRootFoldersAsync();
            return View(mainCategories);
        }
    }
}
