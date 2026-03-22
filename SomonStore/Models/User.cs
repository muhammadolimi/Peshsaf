using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;

        public int RoleId { get; set; }
        public UserRole UserRole { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public decimal BonusBalance { get; set; }

        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
