using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Peshsaf.Models;

namespace Peshsaf.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDbContext _context;

        public HomeController(ShopDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Categories = _context.Categories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToList();

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToList();

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
