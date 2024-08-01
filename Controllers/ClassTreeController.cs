using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ShoperiaDocumentation.Controllers
{
    public class ClassTreeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassTreeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rootFolders = await _context.Folders
                .Where(f => f.ParentId == null)
                .ToListAsync();

            return View(rootFolders);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int parentId)
        {
            var subFolders = await _context.Folders
                .Where(f => f.ParentId == parentId)
                .ToListAsync();

            return Json(subFolders);
        }

        [HttpGet]
        public async Task<IActionResult> GetFolders(int parentId)
        {
            var folders = await _context.Folders
                .Where(f => f.ParentId == parentId)
                .ToListAsync();

            return PartialView("_FolderListPartial", folders);
        }
    }
}
