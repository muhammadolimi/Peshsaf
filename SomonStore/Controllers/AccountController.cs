using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;
using System.Linq;

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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Введите логин и пароль");
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View();
            }

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
            if (!ModelState.IsValid)
                return View(user);

            if (_context.Users.Any(u => u.Login == user.Login))
            {
                ModelState.AddModelError("Login", "Этот логин уже занят");
                return View(user);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Этот email уже используется");
                return View(user);
            }

            var customerRole = _context.UserRoles.FirstOrDefault(r => r.Name == "Customer");

            if (customerRole == null)
            {
                customerRole = new UserRole
                {
                    Name = "Customer",
                    Users = new List<User>()
                };

                _context.UserRoles.Add(customerRole);
                _context.SaveChanges();
            }

            user.RoleId = customerRole.Id;
            user.BonusBalance = 0;
            user.Name ??= "NoName";
            user.Phone ??= "";
            user.Address ??= "";

            _context.Users.Add(user);
            _context.SaveChanges();

            var cart = new Cart
            {
                UserId = user.Id
            };

            _context.Carts.Add(cart);
            _context.SaveChanges();

            return RedirectToAction(nameof(Login));
        }
    }
}