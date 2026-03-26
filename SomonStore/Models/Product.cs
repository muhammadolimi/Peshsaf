using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int? PromotionId { get; set; }
        public Promotion? Promotion { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public decimal GetCurrentPrice()
        {
            if (Promotion == null)
            {
                return Price;
            }

            var now = DateTime.UtcNow;
            var isActive = Promotion.StartDate <= now && Promotion.EndDate >= now;
            if (!isActive || Promotion.Discount <= 0)
            {
                return Price;
            }

            var discountAmount = Price * Promotion.Discount / 100m;
            return Price - discountAmount;
        }
    }
}
