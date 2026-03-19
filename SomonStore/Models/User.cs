using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; }
        
        public string Password { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }
        public UserRole UserRole { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        public decimal BonusBalance { get; set; }

        public Cart Cart { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
