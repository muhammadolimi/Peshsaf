using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel.DataAnnotations;


namespace Peshsaf.Models
{
    public class Category
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int? ParentId { get; set; }
        public Category? Parent { get; set; }

        // Navigation property
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
