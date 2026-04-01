using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Peshsaf.Models;
using Peshsaf.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
                model.Promotions = GetPromotionOptions();
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
                ImageUrl = imagePath,
                // assign optional promotion if provided
                PromotionId = model.PromotionId
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
                model.Promotions = GetPromotionOptions();
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
            // update promotion association
            product.PromotionId = model.PromotionId;

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
                PromotionId = product?.PromotionId,
                Categories = GetCategoryOptions(),
                Promotions = GetPromotionOptions()
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

        private List<SelectListItem> GetPromotionOptions()
        {
            // The first option represents no promotion
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Нет" }
            };
            var promotions = _context.Promotions
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToList();
            list.AddRange(promotions);
            return list;
        }

        #region Category management

        // Display list of categories
        public IActionResult Categories()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var categories = _context.Categories
                .Include(c => c.Parent)
                .OrderBy(c => c.Id)
                .ToList();
            return View("Categories", categories);
        }

        // Render form for creating a new category
        [HttpGet]
        public IActionResult CreateCategory()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var model = BuildCategoryFormViewModel();
            return View("CategoryForm", model);
        }

        // Handle submission of a new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(AdminCategoryFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                model.ParentOptions = GetParentCategoryOptions(model.Id);
                return View("CategoryForm", model);
            }

            var category = new Category
            {
                Name = model.Name,
                ParentId = model.ParentId
            };
            _context.Categories.Add(category);
            _context.SaveChanges();
            TempData["AdminSuccess"] = "Категория добавлена.";
            return RedirectToAction(nameof(Categories));
        }

        // Render form for editing an existing category
        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            var model = BuildCategoryFormViewModel(category);
            return View("CategoryForm", model);
        }

        // Handle submission of category edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(AdminCategoryFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            if (!ModelState.IsValid || model.Id == null)
            {
                model.ParentOptions = GetParentCategoryOptions(model.Id);
                return View("CategoryForm", model);
            }

            var category = _context.Categories.Find(model.Id.Value);
            if (category == null)
            {
                return NotFound();
            }
            category.Name = model.Name;
            category.ParentId = model.ParentId;
            _context.SaveChanges();
            TempData["AdminSuccess"] = "Категория обновлена.";
            return RedirectToAction(nameof(Categories));
        }

        // Delete a category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;

            var category = _context.Categories
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                if (category.Children.Any())
                {
                    TempData["AdminError"] = "Нельзя удалить категорию с подкатегориями.";
                    return RedirectToAction(nameof(Categories));
                }
                if (category.Products.Any())
                {
                    TempData["AdminError"] = "Нельзя удалить категорию, в которой есть товары.";
                    return RedirectToAction(nameof(Categories));
                }
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["AdminSuccess"] = "Категория удалена.";
            }
            return RedirectToAction(nameof(Categories));
        }

        private AdminCategoryFormViewModel BuildCategoryFormViewModel(Category? category = null)
        {
            return new AdminCategoryFormViewModel
            {
                Id = category?.Id,
                Name = category?.Name ?? string.Empty,
                ParentId = category?.ParentId,
                ParentOptions = GetParentCategoryOptions(category?.Id)
            };
        }

        // Build list of parent category options, excluding the current category to prevent
        // a category becoming its own parent.
        private List<SelectListItem> GetParentCategoryOptions(int? currentCategoryId)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "", Text = "Без родителя" } };
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();
            foreach (var c in categories)
            {
                // Skip the current category (if editing) to avoid self reference
                if (currentCategoryId.HasValue && c.Id == currentCategoryId.Value) continue;
                list.Add(new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.ParentId == null ? c.Name : $"{c.Name} (подкатегория)"
                });
            }
            return list;
        }
        #endregion

        #region Promotion management
        // Display list of promotions
        public IActionResult Promotions()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var promotions = _context.Promotions.OrderByDescending(p => p.Id).ToList();
            return View("Promotions", promotions);
        }

        // Render form for creating a promotion
        [HttpGet]
        public IActionResult CreatePromotion()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var model = new AdminPromotionFormViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            };
            return View("PromotionForm", model);
        }

        // Handle creation of promotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePromotion(AdminPromotionFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            if (!ModelState.IsValid)
            {
                return View("PromotionForm", model);
            }
            var promotion = new Promotion
            {
                Name = model.Name,
                Description = model.Description,
                Discount = model.Discount,
                StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc)
            };
            _context.Promotions.Add(promotion);
            _context.SaveChanges();
            TempData["AdminSuccess"] = "Акция добавлена.";
            return RedirectToAction(nameof(Promotions));
        }

        // Render form for editing a promotion
        [HttpGet]
        public IActionResult EditPromotion(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var promotion = _context.Promotions.Find(id);
            if (promotion == null)
            {
                return NotFound();
            }
            var model = new AdminPromotionFormViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                Discount = promotion.Discount,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate
            };
            return View("PromotionForm", model);
        }

        // Handle promotion edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPromotion(AdminPromotionFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            if (!ModelState.IsValid || model.Id == null)
            {
                return View("PromotionForm", model);
            }
            var promotion = _context.Promotions.Find(model.Id.Value);
            if (promotion == null)
            {
                return NotFound();
            }
            promotion.Name = model.Name;
            promotion.Description = model.Description;
            promotion.Discount = model.Discount;
            promotion.StartDate = model.StartDate;
            promotion.EndDate = model.EndDate;
            _context.SaveChanges();
            TempData["AdminSuccess"] = "Акция обновлена.";
            return RedirectToAction(nameof(Promotions));
        }

        // Delete a promotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePromotion(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var promotion = _context.Promotions
                .Include(p => p.Products)
                .FirstOrDefault(p => p.Id == id);
            if (promotion != null)
            {
                // Remove reference from products
                foreach (var product in promotion.Products)
                {
                    product.PromotionId = null;
                }
                _context.Promotions.Remove(promotion);
                _context.SaveChanges();
                TempData["AdminSuccess"] = "Акция удалена.";
            }
            return RedirectToAction(nameof(Promotions));
        }
        #endregion

        #region User management
        // Display list of users
        public IActionResult Users()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var users = _context.Users
                .Include(u => u.UserRole)
                .OrderByDescending(u => u.Id)
                .ToList();
            return View("Users", users);
        }

        // Render form for creating a new user
        [HttpGet]
        public IActionResult CreateUser()
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var model = BuildUserFormViewModel();
            return View("UserForm", model);
        }

        // Handle creation of user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(AdminUserFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            if (!ModelState.IsValid)
            {
                model.Roles = GetRoleOptions();
                return View("UserForm", model);
            }
            // Ensure unique login and email
            if (_context.Users.Any(u => u.Login == model.Login))
            {
                ModelState.AddModelError(nameof(model.Login), "Пользователь с таким логином уже существует.");
                model.Roles = GetRoleOptions();
                return View("UserForm", model);
            }
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким Email уже существует.");
                model.Roles = GetRoleOptions();
                return View("UserForm", model);
            }

            var user = new User
            {
                Login = model.Login,
                Password = model.Password,
                Email = model.Email,
                Name = model.Name,
                Phone = model.Phone,
                Address = model.Address,
                BonusBalance = model.BonusBalance,
                RoleId = model.RoleId
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            // Create cart for user
            _context.Carts.Add(new Cart { UserId = user.Id });
            _context.SaveChanges();
            TempData["AdminSuccess"] = "Пользователь добавлен.";
            return RedirectToAction(nameof(Users));
        }

        // Render form for editing a user
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            var model = BuildUserFormViewModel(user);
            return View("UserForm", model);
        }

        // Handle editing of a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(AdminUserFormViewModel model)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            if (!ModelState.IsValid || model.Id == null)
            {
                model.Roles = GetRoleOptions();
                return View("UserForm", model);
            }
            var user = _context.Users.Find(model.Id.Value);
            if (user == null)
            {
                return NotFound();
            }
            // Check for duplicate login/email if changed
            if (_context.Users.Any(u => u.Id != user.Id && u.Login == model.Login))
            {
                ModelState.AddModelError(nameof(model.Login), "Пользователь с таким логином уже существует.");
                model.Roles = GetRoleOptions();
                return View("UserForm", model);
            }
            if (_context.Users.Any(u => u.Id != user.Id && u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким Email уже существует.");
                model.Roles = GetRoleOptions();
                return View("UserForm", model);
            }
            user.Login = model.Login;
            user.Password = model.Password;
            user.Email = model.Email;
            user.Name = model.Name;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.BonusBalance = model.BonusBalance;
            user.RoleId = model.RoleId;
            _context.SaveChanges();
            TempData["AdminSuccess"] = "Пользователь обновлён.";
            return RedirectToAction(nameof(Users));
        }

        // Delete a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var guard = RequireAdmin();
            if (guard != null) return guard;
            var user = _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Favorites)
                .Include(u => u.Cart)
                    .ThenInclude(c => c.Items)
                .FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                if (user.Orders.Any())
                {
                    TempData["AdminError"] = "Нельзя удалить пользователя с оформленными заказами.";
                    return RedirectToAction(nameof(Users));
                }
                // Remove favourites
                if (user.Favorites.Any())
                {
                    _context.Favorites.RemoveRange(user.Favorites);
                }
                // Remove cart and items
                if (user.Cart != null)
                {
                    if (user.Cart.Items.Any())
                    {
                        _context.CartItems.RemoveRange(user.Cart.Items);
                    }
                    _context.Carts.Remove(user.Cart);
                }
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["AdminSuccess"] = "Пользователь удалён.";
            }
            return RedirectToAction(nameof(Users));
        }

        private AdminUserFormViewModel BuildUserFormViewModel(User? user = null)
        {
            return new AdminUserFormViewModel
            {
                Id = user?.Id,
                Login = user?.Login ?? string.Empty,
                Password = user?.Password ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Name = user?.Name ?? string.Empty,
                Phone = user?.Phone,
                Address = user?.Address,
                BonusBalance = user?.BonusBalance ?? 0,
                RoleId = user?.RoleId ?? 0,
                Roles = GetRoleOptions()
            };
        }

        private List<SelectListItem> GetRoleOptions()
        {
            return _context.UserRoles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                })
                .ToList();
        }
        #endregion
    }
}