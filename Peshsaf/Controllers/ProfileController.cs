using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Peshsaf.Models;

namespace Peshsaf.Controllers
{
    /// <summary>
    /// Controller that displays and updates the current user's profile.  The index
    /// action shows basic account information along with a modal form for editing
    /// editable fields.  UpdateProfile handles persisting changes.
    /// </summary>
    public class ProfileController : Controller
    {
        private readonly ShopDbContext _context;
        public ProfileController(ShopDbContext context)
        {
            _context = context;
        }
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        /// <summary>
        /// Renders the profile page for the current user.  If the user is not logged in,
        /// they will be redirected to the login page.
        /// </summary>
        public IActionResult Index()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefault(u => u.Id == CurrentUserId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(user);
        }
        /// <summary>
        /// Persist changes to the user's editable profile fields.  Only the name,
        /// email, phone and address fields are updated.  Duplicate email
        /// addresses are checked for uniqueness.  After updating, the session
        /// variables reflecting the user's name are refreshed.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(int id, string name, string email, string? phone, string? address)
        {
            if (CurrentUserId == null || CurrentUserId.Value != id)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            // Trim inputs
            name = name?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            phone = phone?.Trim();
            address = address?.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                TempData["ProfileError"] = "Имя и email обязательны.";
                return RedirectToAction(nameof(Index));
            }
            // Ensure email uniqueness
            if (_context.Users.Any(u => u.Id != user.Id && u.Email == email))
            {
                TempData["ProfileError"] = "Пользователь с таким email уже существует.";
                return RedirectToAction(nameof(Index));
            }
            user.Name = name;
            user.Email = email;
            user.Phone = phone;
            user.Address = address;
            _context.SaveChanges();
            // refresh session values
            HttpContext.Session.SetString("UserName", user.Name);
            TempData["ProfileSuccess"] = "Профиль обновлён.";
            return RedirectToAction(nameof(Index));
        }
    }
}