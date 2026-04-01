using System.ComponentModel.DataAnnotations;

namespace Peshsaf.ViewModels
{
    /// <summary>
    /// View model for creating or editing promotions within the admin area.  Promotions
    /// include a name, description, discount percentage and a start/end date.
    /// </summary>
    public class AdminPromotionFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Название акции")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100)]
        [Display(Name = "Скидка, %")]
        public int Discount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; }
    }
}