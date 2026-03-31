using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Peshsaf.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation property
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    }
}
