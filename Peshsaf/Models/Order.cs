using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Peshsaf.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
