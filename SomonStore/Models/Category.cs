using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel.DataAnnotations;


namespace SomonStore.Models
{
    public class Category
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }
        public Category? Parent { get; set; }

        public ICollection<Category> Children { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
