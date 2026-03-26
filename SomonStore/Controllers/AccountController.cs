using Microsoft.AspNetCore.Mvc;
using SomonStore.Models;

namespace SomonStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopDbContext _context;

        public AccountController(ShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Введите логин и пароль.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.RoleId == _context.UserRoles.First(r => r.Name == "Admin").Id ? "Admin" : "Customer");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Login) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.Error = "Заполните логин, пароль и email.";
                return View(user);
            }

            if (_context.Users.Any(u => u.Login == user.Login))
            {
                ViewBag.Error = "Этот логин уже занят.";
                return View(user);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ViewBag.Error = "Этот email уже используется.";
                return View(user);
            }

            var customerRole = _context.UserRoles.First(r => r.Name == "Customer");
            user.RoleId = customerRole.Id;
            user.Name = string.IsNullOrWhiteSpace(user.Name) ? user.Login : user.Name;
            user.Phone ??= string.Empty;
            user.Address ??= string.Empty;
            user.BonusBalance = 0;

            _context.Users.Add(user);
            _context.SaveChanges();

            _context.Carts.Add(new Cart { UserId = user.Id });
            _context.SaveChanges();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", "Customer");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
