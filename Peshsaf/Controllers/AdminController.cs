using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Peshsaf.Models;
using Peshsaf.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace Peshsaf.Controllers
{
    public class AdminController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ShopDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        private IActionResult RequireAdmin()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsAdmin())
            {
                TempData["AdminError"] = "Доступ только для администратора.";
                return RedirectToAction("Index", "Home");
            }

            return null!;
        }

        public IActionResult Index()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var products = _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            return View("ProductForm", BuildFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(AdminProductFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                model.Categories = GetCategoryOptions();
                return View("ProductForm", model);
            }

            string? imagePath = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(stream);
                }

                imagePath = "/images/" + uniqueFileName;
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                CategoryId = model.CategoryId,
                ImageUrl = imagePath
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["AdminSuccess"] = "Товар добавлен.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View("ProductForm", BuildFormViewModel(product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(AdminProductFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            if (!ModelState.IsValid || model.Id == null)
            {
                model.Categories = GetCategoryOptions();
                return View("ProductForm", model);
            }

            var product = _context.Products.Find(model.Id.Value);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Quantity = model.Quantity;
            product.CategoryId = model.CategoryId;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(stream);
                }

                product.ImageUrl = "/images/" + uniqueFileName;
            }

            _context.SaveChanges();

            TempData["AdminSuccess"] = "Товар обновлён.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var product = _context.Products.Find(id);
            if (product != null)
            {
                var cartItems = _context.CartItems.Where(ci => ci.ProductId == id).ToList();
                if (cartItems.Any())
                {
                    _context.CartItems.RemoveRange(cartItems);
                }

                var hasOrders = _context.OrderItems.Any(oi => oi.ProductId == id);
                if (hasOrders)
                {
                    TempData["AdminError"] = "Нельзя удалить товар, который уже есть в оформленных заказах.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["AdminSuccess"] = "Товар удалён.";
            }

            return RedirectToAction(nameof(Index));
        }

        private AdminProductFormViewModel BuildFormViewModel(Product? product = null)
        {
            return new AdminProductFormViewModel
            {
                Id = product?.Id,
                Name = product?.Name ?? string.Empty,
                Description = product?.Description ?? string.Empty,
                Price = product?.Price ?? 0,
                Quantity = product?.Quantity ?? 0,
                CategoryId = product?.CategoryId ?? 0,
                Categories = GetCategoryOptions()
            };
        }

        private List<SelectListItem> GetCategoryOptions()
        {
            return _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.ParentId == null ? c.Name : $"{c.Name} (подкатегория)"
                })
                .ToList();
        }
    }
}