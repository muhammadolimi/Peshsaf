using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace Peshsaf.ViewModels
{
    public class AdminProductFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0, 999999)]
        public decimal Price { get; set; }

        [Range(0, 999999)]
        public int Quantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Optional promotion associated with this product.  When null, the product
        /// will not participate in any promotion.
        /// </summary>
        public int? PromotionId { get; set; }

        [Display(Name = "Фото")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();

        /// <summary>
        /// Available promotions to choose from.  A blank value should represent no
        /// promotion.
        /// </summary>
        public List<SelectListItem> Promotions { get; set; } = new();
    }
}