using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;
using System.Linq;

namespace SomonStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDbContext _context;

        public ProductController(ShopDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .ToList();

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}