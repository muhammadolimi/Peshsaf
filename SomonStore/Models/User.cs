using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Login { get; set; }
        
        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        public int RoleId { get; set; }
        public UserRole UserRole { get; set; }

        [MinLength(3), MaxLength(20)]
        public string Name { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        public decimal BonusBalance { get; set; }

        public Cart Cart { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
