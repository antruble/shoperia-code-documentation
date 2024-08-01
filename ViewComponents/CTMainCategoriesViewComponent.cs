using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using System.Threading.Tasks;

namespace ShoperiaDocumentation.ViewComponents
{
    public class CTMainCategoriesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CTMainCategoriesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var mainCategories = await _context.Folders
                .Where(f => f.ParentId == null)
                .ToListAsync();

            return View(mainCategories);
        }
    }
}
