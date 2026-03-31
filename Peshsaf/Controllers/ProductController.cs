using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Peshsaf.Models;

namespace Peshsaf.Controllers
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

            // 🔍 Поиск
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            Category? selectedCategory = null;
            List<Category> childCategories = new();

            // 📂 Если выбрана категория
            if (categoryId.HasValue)
            {
                // Получаем все вложенные категории
                var categoryIds = GetCategoryTreeIds(categoryId.Value);

                // Фильтр товаров
                query = query.Where(p => categoryIds.Contains(p.CategoryId));

                // Текущая категория
                selectedCategory = _context.Categories
                    .FirstOrDefault(c => c.Id == categoryId.Value);

                // Дочерние категории
                childCategories = _context.Categories
                    .Where(c => c.ParentId == categoryId.Value)
                    .OrderBy(c => c.Name)
                    .ToList();
            }

            // 🌳 Корневые категории (для select)
            var rootCategories = _context.Categories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToList();

            // 📦 Передача во View
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.RootCategories = rootCategories;
            ViewBag.ChildCategories = childCategories;

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

        // 🔁 Рекурсивно собираем все вложенные категории
        private List<int> GetCategoryTreeIds(int parentId)
        {
            var result = new List<int> { parentId };

            var children = _context.Categories
                .Where(c => c.ParentId == parentId)
                .Select(c => c.Id)
                .ToList();

            foreach (var childId in children)
            {
                result.AddRange(GetCategoryTreeIds(childId));
            }

            return result;
        }
    }
}