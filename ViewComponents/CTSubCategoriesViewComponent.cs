using Microsoft.AspNetCore.Mvc;
using ShoperiaDocumentation.Services;

namespace ShoperiaDocumentation.ViewComponents
{
    public class CTSubCategoriesViewComponent : ViewComponent
    {
        private readonly IFileService _fileService;

        public CTSubCategoriesViewComponent(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string selectedRootName, string selectedSubRootName)
        {
            var selectedRootId = await _fileService.GetFolderIdByNameAndParentIdAsync(selectedRootName, null);
            var subCategories = await _fileService.GetFoldersAsync(selectedRootId);
            ViewData["SelectedRootName"] = selectedRootName;
            ViewData["SelectedSubRootName"] = selectedSubRootName;
            return View(subCategories);
        }
    }
}
