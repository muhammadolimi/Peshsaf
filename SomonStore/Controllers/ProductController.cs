using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;

namespace SomonStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDbContext _context;

        public ProductController(ShopDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? search, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                var categoryIds = GetCategoryTreeIds(categoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();

            return View(query.OrderBy(p => p.Name).ToList());
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        private List<int> GetCategoryTreeIds(int parentId)
        {
            var result = new List<int> { parentId };
            var children = _context.Categories.Where(c => c.ParentId == parentId).Select(c => c.Id).ToList();
            foreach (var childId in children)
            {
                result.AddRange(GetCategoryTreeIds(childId));
            }
            return result;
        }
    }
}
