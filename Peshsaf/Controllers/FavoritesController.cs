using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Peshsaf.Models;

namespace Peshsaf.Controllers
{
    /// <summary>
    /// Controller that handles displaying and modifying the list of favourite products for
    /// the currently logged in user.  Favourites are stored in a separate table
    /// linking users and products.  Only authenticated users can access these
    /// endpoints; anonymous users will be redirected to the login page.
    /// </summary>
    public class FavoritesController : Controller
    {
        private readonly ShopDbContext _context;
        public FavoritesController(ShopDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        /// <summary>
        /// Display a list of products the current user has marked as favourites.
        /// </summary>
        public IActionResult Index()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var favorites = _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p.Category)
                .Where(f => f.UserId == CurrentUserId.Value)
                .ToList();
            return View(favorites);
        }

        /// <summary>
        /// Add a product to the current user's favourites.  If the product is already
        /// present, nothing happens.  After adding the item, the user is redirected
        /// back to the previous page (referer) or to the product index.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            // ensure product exists
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }
            var existing = _context.Favorites.FirstOrDefault(f => f.UserId == CurrentUserId.Value && f.ProductId == productId);
            if (existing == null)
            {
                var fav = new Favorite { UserId = CurrentUserId.Value, ProductId = productId };
                _context.Favorites.Add(fav);
                _context.SaveChanges();
            }
            return Redirect(GetReferrerOrFallback());
        }

        /// <summary>
        /// Remove a product from the current user's favourites.  After removal the user
        /// is redirected back to the previous page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var fav = _context.Favorites.FirstOrDefault(f => f.UserId == CurrentUserId.Value && f.ProductId == productId);
            if (fav != null)
            {
                _context.Favorites.Remove(fav);
                _context.SaveChanges();
            }
            return Redirect(GetReferrerOrFallback());
        }

        // Helper to redirect to referring url if available


        private string GetReferrerOrFallback()
        {
            var refUrl = HttpContext.Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(refUrl))
            {
                return refUrl;
            }
            return Url.Action("Index", "Product") ?? "/";
        }
    }
}