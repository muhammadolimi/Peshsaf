using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Peshsaf.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public int MethodId { get; set; }
        public PaymentMethod Method { get; set; } = null!;
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
