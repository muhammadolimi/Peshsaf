using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomonStore.Models;

namespace SomonStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopDbContext _context;

        public CartController(ShopDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        private Cart? GetCurrentCart()
        {
            if (CurrentUserId == null)
            {
                return null;
            }

            return _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.UserId == CurrentUserId.Value);
        }

        public IActionResult Index()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCurrentCart();
            if (cart == null)
            {
                cart = new Cart { UserId = CurrentUserId.Value };
                _context.Carts.Add(cart);
                _context.SaveChanges();
                return View(new List<CartItem>());
            }

            return View(cart.Items.OrderBy(i => i.Product.Name).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            if (quantity < 1)
            {
                quantity = 1;
            }

            var cart = GetCurrentCart();
            if (cart == null)
            {
                cart = new Cart { UserId = CurrentUserId.Value };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var item = _context.CartItems.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            if (item == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var item = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Id == id && ci.Cart.UserId == CurrentUserId.Value);

            if (item == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var item = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Id == id && ci.Cart.UserId == CurrentUserId.Value);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
