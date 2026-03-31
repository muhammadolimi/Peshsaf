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

        [Display(Name = "Фото")]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
    }
}