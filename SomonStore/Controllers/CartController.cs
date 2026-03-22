using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;
using System.Linq;

namespace SomonStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopDbContext _context;

        public CartController(ShopDbContext context)
        {
            _context = context;
        }

        // Временное решение для учебного проекта:
        // берём первую корзину из базы.
        private Cart? GetCurrentCart()
        {
            return _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault();
        }

        public IActionResult Index()
        {
            var cart = GetCurrentCart();

            if (cart == null)
                return View(Enumerable.Empty<CartItem>());

            return View(cart.Items.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId)
        {
            var cart = GetCurrentCart();
            if (cart == null)
                return BadRequest("Корзина не найдена");

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound();

            var item = _context.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1
                };

                _context.CartItems.Add(item);
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = _context.CartItems.Find(id);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}