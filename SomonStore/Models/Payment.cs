using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public DateTime Date { get; set; }

        public decimal Total { get; set; }

        public int MethodId { get; set; }
        public PaymentMethod Method { get; set; }
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
