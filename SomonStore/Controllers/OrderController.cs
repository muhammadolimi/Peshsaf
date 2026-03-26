using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;

namespace SomonStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly ShopDbContext _context;

        public OrderController(ShopDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        public IActionResult Checkout()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.UserId == CurrentUserId.Value);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.UserId == CurrentUserId.Value);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var totalAmount = cart.Items.Sum(i => i.Product.GetCurrentPrice() * i.Quantity);

            var order = new Order
            {
                UserId = CurrentUserId.Value,
                Date = DateTime.UtcNow,
                Status = "Created",
                TotalAmount = totalAmount,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.GetCurrentPrice()
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);
            _context.SaveChanges();

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}
