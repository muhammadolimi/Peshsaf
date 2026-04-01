using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Peshsaf.Models;
using Microsoft.AspNetCore.Http;

namespace Peshsaf.Controllers
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

            // reduce product quantity for each ordered item
            foreach (var item in cart.Items)
            {
                var product = item.Product;
                if (product.Quantity >= item.Quantity)
                {
                    product.Quantity -= item.Quantity;
                }
                else
                {
                    // if for some reason quantity is lower than requested, set to zero
                    product.Quantity = 0;
                }
            }

            _context.Orders.Add(order);
            // clear cart items
            _context.CartItems.RemoveRange(cart.Items);
            _context.SaveChanges();

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // Displays a list of orders placed by the current user
        public IActionResult MyOrders()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == CurrentUserId.Value)
                .OrderByDescending(o => o.Date)
                .ToList();
            return View("MyOrders", orders);
        }
    }
}
