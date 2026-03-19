using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; } 

        public int Discount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
