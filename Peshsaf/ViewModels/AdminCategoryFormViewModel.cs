using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Peshsaf.ViewModels
{
    /// <summary>
    /// View model used by the admin area for creating and editing categories.
    /// Contains a list of available parent categories for selection.
    /// </summary>
    public class AdminCategoryFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Название категории")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Родительская категория")]
        public int? ParentId { get; set; }
        /// <summary>
        /// Drop-down list of parent categories.  The first element should be a
        /// placeholder representing 'no parent'.
        /// </summary>
        public List<SelectListItem> ParentOptions { get; set; } = new();
    }
}