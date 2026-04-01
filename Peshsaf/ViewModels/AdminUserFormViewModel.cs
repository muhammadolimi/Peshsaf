using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Peshsaf.ViewModels
{
    /// <summary>
    /// View model used for creating or editing users from the admin panel.  Includes
    /// validation attributes for required fields and lists of roles to assign.
    /// </summary>
    public class AdminUserFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Логин")]
        public string Login { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Имя")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [MaxLength(255)]
        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [Range(0, 99999999)]
        [Display(Name = "Бонусы")]
        public decimal BonusBalance { get; set; }

        [Required]
        [Display(Name = "Роль пользователя")]
        public int RoleId { get; set; }
        /// <summary>
        /// List of available user roles for assignment in a drop‑down.
        /// </summary>
        public List<SelectListItem> Roles { get; set; } = new();
    }
}