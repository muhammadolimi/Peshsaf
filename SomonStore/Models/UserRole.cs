using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SomonStore.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
