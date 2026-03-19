using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        public decimal TotalAmount { get; set; }

        public Payment? Payment { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
