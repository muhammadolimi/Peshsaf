using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;
using System;
using System.Linq;

namespace SomonStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly ShopDbContext _context;

        public OrderController(ShopDbContext context)
        {
            _context = context;
        }

        public IActionResult Checkout()
        {
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault();

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create()
        {
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault();

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var user = _context.Users.FirstOrDefault();
            if (user == null)
                return BadRequest("Пользователь не найден");

            var totalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            var order = new Order
            {
                UserId = user.Id,
                Date = DateTime.Now,
                Status = "Created",
                TotalAmount = totalAmount,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            _context.CartItems.RemoveRange(cart.Items);

            _context.SaveChanges();

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}